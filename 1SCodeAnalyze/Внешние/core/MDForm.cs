using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using V8Reader.Comparison;

namespace V8Reader.Core
{
    abstract class MDForm : MDObjectBase, IMDTreeItem, ICommandProvider, Editors.IEditable, IHelpProvider
    {
        public enum FormKind
        {
            Ordinary,
            Managed
        }

        public FormKind Kind { get; protected set; }

        public HTMLDocument Help
        {
            get
            {

                if (m_Help == null)
                {
                    m_Help = new HelpProviderImpl(Container, ID + ".1");
                }

                return m_Help.Help;

            }
        }
        public abstract string Module { get; }
        public abstract MDUserDialogBase DialogDef { get; }

        protected MDForm(IV8MetadataContainer Container, string formID) : base()
        {
            var Header = Container.GetElement(formID);
            
            SerializedList StringsBlock = FindStringsBlock(Header.ReadAll(), formID);
            ReadStringsBlock(StringsBlock);
            _Container = Container;

        }

        protected SerializedList FindStringsBlock(String RawContent, String formID)
        {
            int pos = RawContent.IndexOf("{0,0," + formID + "}");
            int ListStart = -1;
            if (pos > 0)
            {
                for (int j = pos - 1; RawContent[j] != '{' && j >= 0; --j)
                {
                    ListStart = j;
                }
            }
            else if (pos == 0)
            {
                ListStart = 0;
            }
            else
                throw new MDStreamFormatException();

            if (ListStart < 0)
                throw new MDStreamFormatException();

            return new SerializedList(RawContent.Substring(ListStart - 1));
        }

        protected override void DeclareProperties()
        {
            base.DeclareProperties();
            var internalProps = base.PropHolder;

            internalProps.Add(PropDef.Create("Dialog", "Форма", DialogDef, Comparison.ToStringComparator.ComparatorObject));
            internalProps.Properties["Dialog"].ValueVisualizer = new DialogVisualizer(DialogDef);

            var moduleProp = PropDef.Create("Module", "Модуль", new V8ModuleProcessor(Module, String.Format("{0}.МодульФормы",Name)));
            moduleProp.ValueVisualizer = new V8ModulePropVisualizer((V8ModuleProcessor)moduleProp.Value);
            internalProps.Add(moduleProp);

        }

        private IV8MetadataContainer _Container;

        protected IV8MetadataContainer Container
        {
            get { return _Container; }
        }

        private HelpProviderImpl m_Help = null;

        ////////////////////////////////////////////////////////
        // static

        public static MDForm Create(IV8MetadataContainer Container, string ElementName)
        {
            var Data = Container.GetElement(ElementName + ".0");

            if (Data.ElemType == MDFileItem.ElementType.Directory)
            {
                return MDOrdinaryForm.Create(Container, ElementName);
            }
            else
            {
                return MDManagedForm.Create(Container, ElementName);
            }
        }

        #region IMDTreeItem implementation

        public virtual string Key
        {
            get { return ID; }
        }

        public virtual string Text
        {
            get { return Name; }
        }

        public virtual AbstractImage Icon
        {
            get { return  IconCollections.MDObjects["Form"]; }
        }

        public virtual bool HasChildren()
        {
            return false;
        }

        public virtual IEnumerable<IMDTreeItem> ChildItems
        {
            get { return null; }
        }

        public virtual IEnumerable<UICommand> Commands
        {
            get
            {
                List<UICommand> cmdList = new List<UICommand>();

                cmdList.Add(new UICommand("Открыть", this, new Action(()=>
                    {
                        var editor = ((Editors.IEditable)this).GetEditor();
                        editor.Edit();

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

        #region IEditable Members

        public virtual Editors.ICustomEditor GetEditor()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    internal enum FormElementClass
    {
        Field,
        Label,
        Button,
        Grid,
        Group,
        Unknown
    }

    abstract class FormElement
    {
        public FormElement(String ElementName, FormElementClass ElemClass)
        {
            m_Class = ElemClass;
        }

        public FormElementClass Class 
        { 
            get { return m_Class; }
        }

        private FormElementClass m_Class;
        public string Name { get; private set; }
        
    }

    class DialogVisualizer : IValueVisualizer
    {

        public DialogVisualizer(MDUserDialogBase dialog)
        {

        }

        #region IValueVisualizer Members

        public System.Windows.Documents.Block FlowContent
        {
            get 
            {
                var run = new System.Windows.Documents.Run(StringContent);
                var p = new System.Windows.Documents.Paragraph(run);
                p.Margin = new System.Windows.Thickness(0);

                return p; 
            }
        }

        public string StringContent
        {
            get { return "Диалоговое окно"; }
        }

        #endregion
    }

}
