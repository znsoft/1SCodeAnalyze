using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using V8Reader.Editors;

namespace V8Reader.Core
{
    class MDReport : MDObjectClass, IMDTreeItem, IEditable, ICommandProvider
    {
        private MDReport() : base()
	    {

	    }

        public static MDReport Create(IV8MetadataContainer Container, SerializedList Content)
        {
            MDReport NewReport = new MDReport();
            NewReport.Container = Container;

            StaticMDIdentifiers ids = new StaticMDIdentifiers();
            ids.AttributesCollection = "7e7123e0-29e2-11d6-a3c7-0050bae0a776";
            ids.TablesCollection     = "b077d780-29e2-11d6-a3c7-0050bae0a776";
            ids.FormsCollection      = "a3b368c0-29e2-11d6-a3c7-0050bae0a776";
            ids.TemplatesCollection  = "3daea016-69b7-4ed4-9453-127911372fe6";

            ReadFromStream(NewReport, Content, ids);

            return NewReport;
        }

        protected override string ObjectModuleFile()
        {
            return this.ID + ".0";
        }

        protected override string HelpFile()
        {
            return this.ID + ".1";
        }

        #region ITreeItem implementation

        List<IMDTreeItem> m_StaticChildren;

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
            get { return IconCollections.MDObjects["Report"]; }
        }

        public bool HasChildren()
        {
            return true;
        }

        public IEnumerable<IMDTreeItem> ChildItems
        {
            get
            {
                if (m_StaticChildren == null)
                {
                    m_StaticChildren = new List<IMDTreeItem>();

                    m_StaticChildren.Add(new StaticTreeNode("Реквизиты", IconCollections.MDObjects["AttributesCollection"], Attributes));
                    m_StaticChildren.Add(new StaticTreeNode("Табличные части", IconCollections.MDObjects["TablesCollection"], Tables));
                    m_StaticChildren.Add(new StaticTreeNode("Формы", IconCollections.MDObjects["FormsCollection"], Forms));
                    m_StaticChildren.Add(new StaticTreeNode("Макеты", IconCollections.MDObjects["TemplatesCollection"], Templates));

                }

                return m_StaticChildren;
            }

        }

        #endregion

        #region ICommandProvider

        public IEnumerable<UICommand> Commands
        {
            get
            {
                List<UICommand> cmdList = new List<UICommand>();

                cmdList.Add(new UICommand("Открыть модуль объекта", this, new Action(() =>
                {
                    var modProc = Properties["Module"].Value as V8ModuleProcessor;

                    modProc.GetEditor().Edit();

                })));

                cmdList.Add(new UICommand("Справочная информация", this, new Action(() =>
                {
                    if (!Help.IsEmpty)
                    {
                        String Path = Help.Location;
                        System.Diagnostics.Process.Start(Path);

                    }

                })));

                return cmdList;
            }
        }

        #endregion

        #region IEditable implementation

        public ICustomEditor GetEditor()
        {
            return new Editors.ReportEditor(this);
        }

        #endregion

        #region IMDPropertyProvider Members

        protected override void DeclareProperties()
        {
            base.DeclareProperties();
            var internalProps = base.PropHolder;

            internalProps.Add(PropDef.Create("Module", "Модуль объекта",
                new V8ModuleProcessor(ObjectModule, "Модуль объекта:" + Name)));

            internalProps.Add(PropDef.Create("Help", "Справочная информация", Help));

        }

        #endregion

    }
}
