using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace V8Reader.Core
{
    abstract class MDClassBase : MDObjectBase
    {
        private IV8MetadataContainer m_Container;
        private MDObjectsCollection<MDForm> _Forms = new MDObjectsCollection<MDForm>();
        private MDObjectsCollection<MDTemplate> _Templates = new MDObjectsCollection<MDTemplate>();

        internal MDObjectsCollection<MDTemplate> Templates
        {
            get { return _Templates; }
        }

        internal MDObjectsCollection<MDForm> Forms
        {
            get { return _Forms; }
        }

        protected IV8MetadataContainer Container
        {
            get { return m_Container; }
            set { m_Container = value; }
        }
    }

    abstract class MDTypeDeclarator : MDClassBase
    {
        public override string ToString()
        {
            return Name;
        }
    }

    abstract class MDObjectClass : MDTypeDeclarator, IHelpProvider
    {

        public MDObjectsCollection<MDAttribute> Attributes
        {
            get { return _Attributes; }
        }
        
        public MDObjectsCollection<MDTable> Tables
        {
            get { return _Tables; }
        }
        
        public String ObjectModule
        {
            get
            {
                MDFileItem DirElem;

                try
                {
                    DirElem = Container.GetElement(ObjectModuleFile());
                }
                catch (System.IO.FileNotFoundException)
                {
                    return String.Empty; // Модуля нет
                }

                if (DirElem.ElemType == MDFileItem.ElementType.Directory)
                {

                    try
                    {
                        var textElem = DirElem.GetElement("text");
                        return textElem.ReadAll();
                    }
                    catch (System.IO.FileNotFoundException)
                    {
                        return String.Empty;
                    }

                }
                else
                {
                    return DirElem.ReadAll(); // если модуль зашифрован, то будет нечитаемый текст
                }
            }
        }
        
        public HTMLDocument Help
        {
            get 
            {
                if (_Help == null)
                {
                    _Help = new HelpProviderImpl(Container, HelpFile());
                }

                return _Help.Help;
            }
        }

        protected struct StaticMDIdentifiers
        {
            public string AttributesCollection;
            public string TablesCollection;
            public string FormsCollection;
            public string TemplatesCollection;
        }

        protected static void ReadFromStream(MDObjectClass NewMDObject, SerializedList ProcData, StaticMDIdentifiers ids)
        {
            SerializedList Content = ProcData.DrillDown(3);

            NewMDObject.ReadStringsBlock(Content.DrillDown(3));

            const int start = 3;
            int ChildCount = Int32.Parse(Content.Items[2].ToString());

            for (int i = 0; i < ChildCount; ++i)
            {
                SerializedList Collection = (SerializedList)Content.Items[start + i];

                String CollectionID = Collection.Items[0].ToString();
                int ItemsCount = Int32.Parse(Collection.Items[1].ToString());

                for (int itemIndex = 2; itemIndex < (2 + ItemsCount); ++itemIndex)
                {
                    if (CollectionID == ids.AttributesCollection)
                    {
                        NewMDObject.Attributes.Add(new MDAttribute((SerializedList)Collection.Items[itemIndex]));
                    }
                    else if (CollectionID == ids.TablesCollection)
                    {
                        NewMDObject.Tables.Add(new MDTable((SerializedList)Collection.Items[itemIndex]));
                    }
                    else if (CollectionID == ids.FormsCollection)
                    {
                        NewMDObject.Forms.Add(MDForm.Create(NewMDObject.Container, Collection.Items[itemIndex].ToString()));
                    }
                    else if (CollectionID == ids.TemplatesCollection)
                    {
                        NewMDObject.Templates.Add(new MDTemplate(NewMDObject.Container, Collection.Items[itemIndex].ToString()));
                    }

                }

            }
        }

        abstract protected string ObjectModuleFile();
        abstract protected string HelpFile();

        

        private HelpProviderImpl _Help = null;
        private MDObjectsCollection<MDAttribute> _Attributes = new MDObjectsCollection<MDAttribute>();
        private MDObjectsCollection<MDTable> _Tables = new MDObjectsCollection<MDTable>();

    }

    interface IHelpProvider
    {
        HTMLDocument Help
        {
            get;
        }
    }

    class HelpProviderImpl
    {
        public HelpProviderImpl(IV8MetadataContainer Container, string HelpFile)
        {
            _Container = Container;
            _HelpFile = HelpFile;
        }

        public HTMLDocument Help
        {
            get
            {
                if (_HelpDoc == null)
                {
                    try
                    {
                        var HelpItem = _Container.GetElement(_HelpFile);
                        var Stream = new SerializedList(HelpItem.ReadAll());

                        _HelpDoc = new HTMLDocument(Stream);

                    }
                    catch (System.IO.FileNotFoundException)
                    {
                        _HelpDoc = new HTMLDocument();
                    }
                }

                return _HelpDoc;

            }
        }

        private string _HelpFile;
        private IV8MetadataContainer _Container;
        private HTMLDocument _HelpDoc;

    }

}
