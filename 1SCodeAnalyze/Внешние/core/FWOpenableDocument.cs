using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using V8Reader.Editors;

namespace V8Reader.Core
{
    // Базовый класс для макетов, открываемых с помощью "Работы с файлами" (File Workshop)
    // Основное назначение - обертка для сырых данных, передаваемых в File Workshop

    abstract class FWOpenableDocument : TemplateDocument
    {
        public FWOpenableDocument(MDTemplate OwnerTemplate) : base(OwnerTemplate) { }

        public override ICustomEditor GetEditor()
        {
            return new Editors.FileWorkshopEditor(this);
        }

        public virtual String Extract()
        {
            String Result = DefaultExtractionPath();

            using (var SourceStream = GetDataStream())
            {
                using (var DestStream = new System.IO.FileStream(Result, System.IO.FileMode.OpenOrCreate))
                {
                    SourceStream.CopyTo(DestStream);
                }
            }

            if (AutoExtractionCleanup)
            {
                Utils.TempFileCleanup.RegisterTempFile(Result);
            }

            return Result;

        }

        protected bool AutoExtractionCleanup
        {
            get
            {
                return true;
            }
        }

        protected virtual string DefaultExtractionPath()
        {
            return System.IO.Path.GetTempPath() + FWOpenableName;
        }

        protected System.IO.Stream GetDataStream()
        {
            var FileName = GetFileName();
            MDFileItem FileElement;

            try
            {
                FileElement = Reader.GetElement(FileName);
            }
            catch (System.IO.FileNotFoundException exc)
            {
                throw new MDObjectIsEmpty(Owner.Kind.ToString(), exc);
            }

            if (FileElement.ElemType == MDFileItem.ElementType.File)
            {
                return FileElement.GetStream();
            }
            else
            {
                throw new MDObjectIsEmpty(Owner.Kind.ToString());
            }
        }

        private String FWOpenableName
        {
            get
            {

                String FileExt = "";

                switch (Owner.Kind)
                {
                    case MDTemplate.TemplateKind.Moxel:
                        FileExt = ".mxl";
                        break;
                    case MDTemplate.TemplateKind.Text:
                        FileExt = ".txt";
                        break;
                    case MDTemplate.TemplateKind.GEOSchema:
                        FileExt = ".geo";
                        break;
                    case MDTemplate.TemplateKind.GraphicChart:
                        FileExt = ".grs";
                        break;
                    case MDTemplate.TemplateKind.DataCompositionSchema:
                        FileExt = ".txt";
                        break;
                    case MDTemplate.TemplateKind.DCSAppearanceTemplate:
                        FileExt = ".txt";
                        break;
                    default:
                        throw new NotSupportedException();
                }

                return System.IO.Path.GetRandomFileName() + FileExt;

            }
        }

        protected virtual String GetFileName()
        {
            return Owner.ID + ".0";
        }

        private bool IsEmpty()
        {
            try
            {
                var FileElement = Reader.GetElement(GetFileName());
                if (FileElement.ElemType == MDFileItem.ElementType.File)
                {
                    return false;
                }
                else
                {
                    return true;
                }

            }
            catch (System.IO.FileNotFoundException)
            {
                return true;
            }
        }

        #region IComparableItem Members

        public override bool CompareTo(object Comparand)
        {
            FWOpenableDocument cmpDoc = Comparand as FWOpenableDocument;

            bool docEmpty = cmpDoc == null ? true : cmpDoc.IsEmpty();
            bool CurrentIsEmpty = this.IsEmpty();

            if (cmpDoc != null)
            {
                if (docEmpty)
                {
                    return CurrentIsEmpty;
                }
                else if (!CurrentIsEmpty)
                {
                    return InternalCompare(cmpDoc);

                }
                else
                {
                    return true;
                }
            }
            else
            {
                return CurrentIsEmpty;
            }

        }

        protected virtual bool InternalCompare(FWOpenableDocument cmpDoc)
        {
            Comparison.StreamComparator sc = new Comparison.StreamComparator();
            return sc.CompareStreams(GetDataStream(), cmpDoc.GetDataStream());
        }

        public override Comparison.IDiffViewer GetDifferenceViewer(object Comparand)
        {
            FWOpenableDocument cmpDoc = Comparand as FWOpenableDocument;

            var DiffViewer = new Comparison.FWDiffViewer(this, cmpDoc);

            return DiffViewer;

        }

        #endregion

    }
}