using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using V8Reader.Editors;

namespace V8Reader.Core
{
    class MDOrdinaryForm : MDForm, IMDTreeItem
    {

        private MDOrdinaryForm(IV8MetadataContainer Container, string formID) : base(Container, formID) { }

        public override ICustomEditor GetEditor()
        {
            return new Editors.OrdinaryFormEditor(this);
        }

        override public String Module
        {
            get
            {
                LoadIfNeeded();
                return m_ModuleText;
            }
        }

        public override MDUserDialogBase DialogDef
        {
            get 
            {
                LoadIfNeeded();
                return m_DialogDef;
            }
        }

        private void LoadIfNeeded()
        {
            if (!m_Loaded)
            {
                var DirElem = Container.GetElement(this.ID + ".0");
                var textElem = DirElem.GetElement("module");

                m_ModuleText = textElem.ReadAll();

                textElem = DirElem.GetElement("form");
                m_DialogDef = new SimpleDialogStub(new SerializedList(textElem.ReadAll()));

                m_Loaded = true;
            }
        }

        private string m_ModuleText;
        private MDUserDialogBase m_DialogDef;
        private bool m_Loaded;


        /// static
        /// 

        new public static MDOrdinaryForm Create(IV8MetadataContainer Container, string formID)
        {
            return new MDOrdinaryForm(Container, formID);
        }

    }
}
