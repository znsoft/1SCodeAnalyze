using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using V8Reader.Editors;

namespace V8Reader.Core
{
    partial class MDDataProcessor : MDObjectClass, IMDTreeItem, IEditable, ICommandProvider
    {

        private MDDataProcessor() : base()
        {
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
            get { return  IconCollections.MDObjects["DataProcessor"]; }
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

                    m_StaticChildren.Add(new StaticTreeNode("Реквизиты",  IconCollections.MDObjects["AttributesCollection"], Attributes));
                    m_StaticChildren.Add(new StaticTreeNode("Табличные части",  IconCollections.MDObjects["TablesCollection"], Tables));
                    m_StaticChildren.Add(new StaticTreeNode("Формы",  IconCollections.MDObjects["FormsCollection"], Forms));
                    m_StaticChildren.Add(new StaticTreeNode("Макеты",  IconCollections.MDObjects["TemplatesCollection"], Templates));

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
            return new Editors.DataProcEditor(this);
        }

        #endregion

        #region IMDPropertyProvider Members

        protected override void DeclareProperties()
        {
            base.DeclareProperties();
            var internalProps = base.PropHolder;

            var moduleProcessor        = new V8ModuleProcessor(ObjectModule, "Модуль объекта:" + Name);
            var moduleProp             = PropDef.Create("Module", "Модуль объекта", moduleProcessor);
            moduleProp.ValueVisualizer = new Comparison.V8ModulePropVisualizer(moduleProcessor);
            internalProps.Add(moduleProp);

            const string kHelpTitle  = "Справочная информация";
            var helpProp             = PropDef.Create("Help", kHelpTitle, Help);
            helpProp.ValueVisualizer = new Comparison.HelpPropVisualizer(Help, kHelpTitle);
            internalProps.Add(helpProp);

            
        }

        #endregion

    }

}
