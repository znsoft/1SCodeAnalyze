using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace V8Reader.Core
{
    
    abstract class MDObjectBase : IMDPropertyProvider
    {

        public MDObjectBase() { }
        
        public MDObjectBase(String ObjID) {
            ID = ObjID;
        }

        public MDObjectBase(SerializedList lst)
        {
            ReadStringsBlock(lst);
        }

        public string ID { get; private set; }
        
        public string Name { get; private set; }
        public string Synonym { get; private set; }
        public string Comment { get; protected set; }
        
        protected void ReadStringsBlock(SerializedList StringBlock)
        {
            ID = StringBlock.Items[1].Items[2].ToString();
            Name = StringBlock.Items[2].ToString();

            if (StringBlock.Items[3].Items.Count > 1)
            {
                Synonym = StringBlock.Items[3].Items[2].ToString();
            }
            else
            {
                Synonym = "";
            }

            Comment = StringBlock.Items[4].ToString();
        }


        #region IMDPropertyProvider Members

        private PropertyHolder m_Props;

        protected PropertyHolder PropHolder
        {
            get
            {
                if (m_Props == null)
                {
                    m_Props = new PropertyHolder();
                    DeclareProperties();
                }

                return m_Props;
            }
        }

        protected virtual void DeclareProperties()
        {
            PropHolder.Add("ID", "ID", ID);
            PropHolder.Add("Name", "Имя", Name);
            PropHolder.Add("Synonym", "Синоним", Synonym);
            PropHolder.Add("Comment", "Комментарий", Comment);

        }

        virtual public IDictionary<string, PropDef> Properties
        {
            get 
            {
                return PropHolder.Properties;
            }
        }

        virtual public object GetValue(string Key)
        {
            return PropHolder.GetValue(Key);
        }


        #endregion
    }
    
    class MDAttribute : MDObjectBase, IMDTreeItem//, Comparison.IComparableItem
    {
        public MDAttribute(SerializedList attrDestription)
        {
            m_RawContent = attrDestription;

            var attrObj = (SerializedList)(attrDestription.Items[0].Items[1]);
            var test = attrObj.Items[0].ToString();

            SerializedList StringsBlock;
            SerializedList Pattern;
            
            if (test == "2")
            {
                StringsBlock = (SerializedList)attrObj.Items[1];
                Pattern = (SerializedList)attrObj.Items[2];
            }
            else
            {
                StringsBlock = (SerializedList)(attrObj.Items[1].Items[1]);
                Pattern = (SerializedList)(attrObj.Items[1].Items[2]);
            }
            
            ReadStringsBlock(StringsBlock);
            m_typeDef = V8TypeDescription.ReadFromList(Pattern);


        }

        private SerializedList m_RawContent;

        private V8TypeDescription m_typeDef;

        #region IMDTreeItem

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
            get {return IconCollections.MDObjects["Attribute"];}
        }

        public bool HasChildren()
        {
            return false;
        }

        public IEnumerable<IMDTreeItem> ChildItems
        {
            get { return null; }
        }


        #endregion

        #region IComparableItem Members

        //public bool CompareTo(object Comparand)
        //{
        //    var Attrib = Comparand as MDAttribute;
        //    if (Attrib == null)
        //        return false;

        //    return m_RawContent.ToString() == Attrib.m_RawContent.ToString();

        //}

        //public Comparison.IDiffViewer GetDifferenceViewer(object Comparand)
        //{
        //    return null;
        //}

        #endregion

        protected override void DeclareProperties()
        {
            base.DeclareProperties();
            var internalProps = base.PropHolder;

            var typeProp = PropDef.Create("TypeDef", "Тип", m_typeDef);
            internalProps.Add(typeProp);

            //typeProp = PropDef.Create("RawContent", "Внутреннее представление", m_RawContent, Comparison.ToStringComparator.ComparatorObject);
            //internalProps.Add(typeProp);

        }

    }

    class MDTable : MDObjectBase, IMDTreeItem
    {
        public MDTable(SerializedList tableDescription)
        {
            m_RawContent = tableDescription;
            var StringsBlock = ((SerializedList)tableDescription).DrillDown(4);
            ReadStringsBlock(StringsBlock);

            m_Attributes = new MDObjectsCollection<MDAttribute>();

            if (tableDescription.Items.Count > 2)
            {
                var AttributeList = (SerializedList)tableDescription.Items[2];

                FillAttributeCollection(AttributeList);

            }

        }

        public MDObjectsCollection<MDAttribute> Attributes
        {
            get { return m_Attributes; }
        }
        
        private void FillAttributeCollection(SerializedList AttrList)
        {
            for (int i = 2; i < AttrList.Items.Count(); ++i)
            {
                Attributes.Add(new MDAttribute((SerializedList)AttrList.Items[i]));
            }
        }

        private SerializedList m_RawContent;
        private MDObjectsCollection<MDAttribute> m_Attributes;

        #region MDTreeItem

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
            get { return  IconCollections.MDObjects["Table"]; }
        }

        public bool HasChildren()
        {
            return true;
        }

        public IEnumerable<IMDTreeItem> ChildItems
        {
            get { return Attributes; }
        }

        #endregion

    }

#region Exceptions

    class MDStreamFormatException : Exception
    {
        public MDStreamFormatException() : base("Stream format error") { }
    }

    class MDObjectIsEmpty : Exception
    {
        public MDObjectIsEmpty(String objName)
        {

        }

        public MDObjectIsEmpty(String objName, Exception InnerException)
            : base(String.Format("Object {0} is empty", objName), InnerException)
        {

        }
    }

#endregion

}
