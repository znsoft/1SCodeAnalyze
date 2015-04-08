using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using V8Reader.Editors;

namespace V8Reader.Core
{
    class MDTemplate : MDObjectBase, IMDTreeItem, ICommandProvider, IEditable
    {
        public enum TemplateKind
        {
            Moxel = 0,
            Text = 4,
            BinaryData = 1,
            ActiveDocument = 2,
            HTMLDocument = 3,
            GEOSchema = 5,
            GraphicChart = 8,
            DataCompositionSchema = 6,
            DCSAppearanceTemplate = 7
        }

        public MDTemplate(IV8MetadataContainer MDContainer, String ObjID)
        {
            _Container = MDContainer;

            SerializedList header = new SerializedList(_Container.GetElement(ObjID).ReadAll());
            Kind = (TemplateKind)Enum.Parse(typeof(TemplateKind), header.Items[1].Items[1].ToString());

            ReadStringsBlock((SerializedList)header.Items[1].Items[2]);

            switch (Kind)
            {
                case MDTemplate.TemplateKind.Moxel:
                case MDTemplate.TemplateKind.Text:
                case MDTemplate.TemplateKind.GEOSchema:
                case MDTemplate.TemplateKind.GraphicChart:
                case MDTemplate.TemplateKind.DCSAppearanceTemplate:
                    m_Document = new PersistedTemplateStub(this);
                    break;
                case MDTemplate.TemplateKind.BinaryData:
                    m_Document = new BinaryDataDocument(this);
                    break;
                case MDTemplate.TemplateKind.HTMLDocument:
                    m_Document = new HTMLTemplate(this);
                    break;
                case MDTemplate.TemplateKind.DataCompositionSchema:
                    m_Document = new DCSSchemaDocument(this);
                    break;
                default:
                    break;
            }

        }

        public TemplateKind Kind { get; protected set; }

        private IV8MetadataContainer _Container;

        public IV8MetadataContainer Container
        {
            get { return _Container; }
        }

        #region IMDTree Implementation

        public String Key
        {
            get { return ID; }
        }

        public String Text
        {
            get { return Name; }
        }

        public AbstractImage Icon
        {
            get { return  IconCollections.MDObjects["Template"]; }
        }

        public virtual bool HasChildren()
        {
            return false;
        }

        public virtual IEnumerable<IMDTreeItem> ChildItems
        {
            get { return null; }
        }

        public IEnumerable<UICommand> Commands
        {
            get
            {
                List<UICommand> cmdList = new List<UICommand>();

                cmdList.Add(new UICommand("Открыть", this, new Action(() =>
                {
                    var editor = ((Editors.IEditable)this).GetEditor();
                    editor.Edit();

                })));

                return cmdList;
            }
        }

        #endregion

        protected TemplateDocument m_Document;

        protected override void DeclareProperties()
        {
            base.DeclareProperties();
            PropHolder.Add("Template", "Макет", m_Document);
        }

        public ICustomEditor GetEditor()
        {
            if (m_Document != null)
                return m_Document.GetEditor();
            else
                throw new NotSupportedException("Редактирование макета данного типа не поддерживается");
        }

    }
}
