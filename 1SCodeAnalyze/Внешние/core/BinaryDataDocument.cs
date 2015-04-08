using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using V8Reader.Editors;

namespace V8Reader.Core
{
    class BinaryDataDocument : TemplateDocument
    {
        public BinaryDataDocument(MDTemplate OwnerTemplate) : base(OwnerTemplate)
        {           
        }

        public System.IO.Stream GetStream()
        {
            MDFileItem Container = Reader.GetElement(GetFileName());
            SerializedList lst = new SerializedList(Container.ReadAll());

            var Base64 = lst.Items[1].Items[0].ToString();

            StringBuilder reader = new StringBuilder(Base64);
            reader.Remove(0, 8);
            Byte[] byteArr = System.Convert.FromBase64String(reader.ToString());

            MemoryStream MemStream = new MemoryStream(byteArr);

            return MemStream;

        }

        private String GetFileName()
        {
            return Owner.ID + ".0";
        }

        public override ICustomEditor GetEditor()
        {
            return new Editors.BinaryTemplateEditor(this);
        }

        #region IComparableItem Members

        public override bool CompareTo(object Comparand)
        {
            BinaryDataDocument cmpDoc = Comparand as BinaryDataDocument;
            if (cmpDoc != null)
            {
                Comparison.StreamComparator sc = new Comparison.StreamComparator();

                return sc.CompareStreams(GetStream(), cmpDoc.GetStream());
            }
            else
            {
                return false;
            }
        }

        public override Comparison.IDiffViewer GetDifferenceViewer(object Comparand)
        {
            return null;
        }

        #endregion

    }
}
