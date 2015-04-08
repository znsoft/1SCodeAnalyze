using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using V8Reader.Editors;

namespace V8Reader.Core
{
    class MDManagedForm : MDForm, IMDTreeItem
    {
        private MDManagedForm(IV8MetadataContainer Container, string formID) : base(Container, formID) { }

        private void LoadFormContent()
        {
            var FileElem = Container.GetElement(ID + ".0");

            var stream = new SerializedList(FileElem.ReadAll());

            m_Elements = new ManagedFormElements();
            m_Elements.ReadFromList((SerializedList)stream.Items[1]);
            m_ModuleText = stream.Items[2].ToString();
            LoadAttributes((SerializedList)stream.Items[3]);

            m_DialogDef = new SimpleDialogStub(CreateDialogDefList(stream));

            m_Loaded = true;
        }

        private static SerializedList CreateDialogDefList(SerializedList stream)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{2,");
            sb.Append(((SerializedList)stream.Items[1]).ToString());
            sb.Append(',');
            sb.Append(((SerializedList)stream.Items[3]).ToString());
            sb.Append('}');

            return new SerializedList(sb.ToString());

        }

        public override MDUserDialogBase DialogDef
        {
            get 
            {
                if (!m_Loaded)
                    LoadFormContent();

                return m_DialogDef;

            }

        }

        override public String Module 
        {
            get
            {
                if (!m_Loaded)
                    LoadFormContent();

                return m_ModuleText;
            }
        }

        public ManagedFormElements Elements
        {
            get
            {
                if (!m_Loaded)
                    LoadFormContent();

                return m_Elements;
            }
        }

        public MDObjectsCollection<ManagedFormAttribute> Attributes
        {
            get
            {
                if (!m_Loaded)
                    LoadFormContent();

                return m_Attributes;
            }
        }

        public override ICustomEditor GetEditor()
        {
            return new Editors.ManagedFormEditor(this);
        }

        private ManagedFormElements m_Elements = null;
        private MDObjectsCollection<ManagedFormAttribute> m_Attributes = null;
        private string m_ModuleText;
        private MDUserDialogBase m_DialogDef;

        private bool m_Loaded = false;
        
        private void LoadAttributes(SerializedList attrSection)
        {
            int count = Int32.Parse(attrSection.Items[1].ToString());

            if (count > 0)
                m_Attributes = new MDObjectsCollection<ManagedFormAttribute>();
            else
                return;

            for (int i = 0; i < count; i++)
            {
                m_Attributes.Add(ManagedFormAttribute.CreateFromList((SerializedList)attrSection.Items[i+2]));
            }
        }

        /// static
        /// 

        new public static MDManagedForm Create(IV8MetadataContainer Container, string formID)
        {
            return new MDManagedForm(Container, formID);
        }

    }

    #region Elements

    class ManagedFormElement : FormElement, IMDTreeItem
    {
        public ManagedFormElement(String ElementName, String ElementTitle, ManagedFormElementType Type) : base(ElementName, Type.Class) 
        {
            m_ElementType = Type;
            Title = ElementTitle;
        }

        public ManagedFormElement(String ElementName, String ElementTitle, ManagedFormElementType Type, ManagedFormElements Children)
            : base(ElementName, Type.Class)
        {
            m_ChildItems = Children;
            m_ElementType = Type;
            Title = ElementTitle;
        }

        public ManagedFormElements ChildElements
        {
            get
            {
                return m_ChildItems;
            }
        }

        public ManagedFormElementType Type
        {
            get
            {
                return m_ElementType;
            }
        }

        public string Title { get; private set; }

        private ManagedFormElements m_ChildItems = null;
        private ManagedFormElementType m_ElementType;

        #region IMDTreeItem implementation

        public string Key
        {
            get { return Name; }
        }

        public string Text
        {
            get { return Title; }
        }

        public AbstractImage Icon
        {
            get 
            {
                String Key;

                switch (Class)
                {
                    case FormElementClass.Field:
                    case FormElementClass.Label:
                    case FormElementClass.Unknown:
                        Key = "Element";
                        break;
                    case FormElementClass.Button:
                        Key = "Button";
                        break;
                    case FormElementClass.Group:
                        Key = "Group";
                        break;
                    case FormElementClass.Grid:
                        Key = "Table";
                        break;
                    default:
                        Key = "Element";
                        break;
                }

                return IconCollections.ManagedForm[Key];
            }
        }

        public bool HasChildren()
        {
            return m_ChildItems != null;
        }

        public IEnumerable<IMDTreeItem> ChildItems
        {
            get { return m_ChildItems; }
        }

        #endregion
    }

    class ManagedFormElements : IEnumerable<ManagedFormElement>
    {
        public ManagedFormElements()
	    {

	    }

        public ManagedFormElements(MDObjectsCollection<ManagedFormElement> items)
        {
            m_Items = items;
            m_InitPerformed = true;
        }

        public void ReadFromList(SerializedList elementsList)
        {
            if (m_InitPerformed)
                throw new InvalidOperationException("Collection can not be reloaded");

            bool uuidFound = false;
            string uuid = null;
            int idIndex = 0;
            System.Text.RegularExpressions.Regex RegExp = new System.Text.RegularExpressions.Regex(@"\w{8}-\w{4}-\w{4}-\w{4}-\w{12}");

            for (int i = 0; i < elementsList.Items.Count; ++i)
            {

                var curElem = elementsList.Items[i];
                if (curElem.GetType() == typeof(SerializedItem))
                {
                    String content = curElem.ToString();
                    if (content.Length == 36 && content != "00000000-0000-0000-0000-000000000000" && RegExp.IsMatch(content))
                    {
                        uuid = content;
                        uuidFound = true;
                        idIndex = i;
                        break;
                    }
                }

            }

            if (uuidFound)
            {
                LoadElements(elementsList, m_Items, idIndex, uuid);
            }

            m_InitPerformed = true;
        }

        private void LoadElements(SerializedList elementsList, MDObjectsCollection<ManagedFormElement> itemsPrototype, int idIndex, string uuid = null)
        {

            int itemCount = Int32.Parse(elementsList.Items[idIndex - 1].ToString());

            int iterationCount = itemCount * 2;

            for (int i = 0; i < iterationCount; ++i)
            {
                int index = idIndex + i;
                if (index >= elementsList.Items.Count)
                    break;

                SerializedItem item = elementsList.Items[index];

                if (item.GetType() == typeof(SerializedItem))
                {
                    uuid = item.ToString();
                }
                else
                {
                    int offset;
                    SerializedList lst = (SerializedList)item;
                    if (lst.Items[4].ToString() == "0")
                    {
                        offset = 0;
                    }
                    else
                    {
                        offset = 1;
                    }

                    String ElementName;
                    String ElementSubtype;

                    if (uuid == "143c00f7-a42d-4cd7-9189-88e4467dc768" || uuid == "a9f3b1ac-f51b-431e-b102-55a69acdecad")
                    {
                        ElementName = lst.Items[6].ToString();
                        ElementSubtype = (uuid == "a9f3b1ac-f51b-431e-b102-55a69acdecad") ? lst.Items[5].ToString() : lst.Items[7].ToString();
                    }
                    else
                    {
                        ElementName = lst.Items[6 + offset].ToString();
                        ElementSubtype = lst.Items[5 + offset].ToString();
                    }

                    var ElementClass = ManagedFormElementType.ParseTypeID(uuid);
                    
                    MDObjectsCollection<ManagedFormElement> ChildrenPrototype = null;

                    if (lst.Items.Count > 22 && (ElementClass == FormElementClass.Group || ElementClass == FormElementClass.Grid))
                    {
                        System.Text.RegularExpressions.Regex RegExp = new System.Text.RegularExpressions.Regex(@"\w{8}-\w{4}-\w{4}-\w{4}-\w{12}");

                        for (int j = 22; j < lst.Items.Count; ++j)
                        {
                            String content = lst.Items[j].ToString();
                            if (content.Length == 36 && content != "00000000-0000-0000-0000-000000000000" && RegExp.IsMatch(content))
                            {
                                // load children

                                ChildrenPrototype = new MDObjectsCollection<ManagedFormElement>();

                                LoadElements(lst, ChildrenPrototype, j);
                                break;
                            }
                        }
                    }

                    ManagedFormElement element;
                    var ElementType = new ManagedFormElementType(ElementClass, Int32.Parse(ElementSubtype));
                    if (ChildrenPrototype == null)
                    {
                        element = new ManagedFormElement(ElementName, ElementName, ElementType);
                    }
                    else
                    {
                        ManagedFormElements Children = new ManagedFormElements(ChildrenPrototype);
                        element = new ManagedFormElement(ElementName, ElementName, ElementType, Children);
                    }

                    itemsPrototype.Add(element);

                }

            }
        }


        public ManagedFormElement this[int index] 
        { 
            get
            {
                return m_Items[index];
            }

        }

        private bool m_InitPerformed = false;
        private MDObjectsCollection<ManagedFormElement> m_Items = new MDObjectsCollection<ManagedFormElement>();
    
        public IEnumerator<ManagedFormElement> GetEnumerator()
        {
            return m_Items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return m_Items.GetEnumerator();
        }
    }

    struct ManagedFormElementType
    {
        public ManagedFormElementType(FormElementClass Class, int TypeToken)
        {
            m_Class = Class;
            m_subType = TypeToken;
        }

        public FormElementClass Class { get { return m_Class; } }
        public int TypeId { get { return m_subType; } }

        public override String ToString()
        {
            String result;

            switch (m_Class)
            {
                case FormElementClass.Field:
                    switch (m_subType)
                    {
                        case 1:
                            result = "Поле ввода";
                            break;
                        case 2:
                            result = "Поле надписи";
                            break;
                        case 3:
                            result = "Поле флажка";
                            break;
                        case 4:
                            result = "Поле картинки";
                            break;
                        case 5:
                            result = "Поле переключателя";
                            break;
                        case 6:
                            result = "Поле табличного документа";
                            break;
                        case 7:
                            result = "Поле текстового документа";
                            break;
                        case 15:
                            result = "Поле HTML документа";
                            break;
                        default:
                            result = "Field_" + m_subType;
                            break;
                    }

                    break;

                case FormElementClass.Group:
                    switch (m_subType)
                    {
                        case 0:
                            result = "Группа - командная панель";
                            break;
                        case 1:
                            result = "Группа подменю";
                            break;
                        case 2:
                            result = "Группа колонок";
                            break;
                        case 3:
                            result = "Группа - страницы";
                            break;
                        case 4:
                            result = "Группа - страница";
                            break;
                        case 5:
                            result = "Обычная группа";
                            break;
                        case 6:
                            result = "Группа кнопок";
                            break;
                        default:
                            result = "Group_" + m_subType;
                            break;
                    }

                    break;

                case FormElementClass.Button:

                    switch (m_subType)
                    {
                        case 0:
                            result = "Кнопка командной панели";
                            break;
                        case 1:
                            result = "Кнопка (обычная кнопка)";
                            break;
                        case 2:
                            result = "Кнопка (гиперссылка)";
                            break;
                        default:
                            result = "Button_" + m_subType;
                            break;
                    }

                    break;

                case FormElementClass.Label:

                    switch (m_subType)
                    {
                        case 0:
                            result = "Декорация (надпись)";
                            break;
                        case 1:
                            result = "Декорация (картинка)";
                            break;
                        default:
                            result = "Label_" + m_subType;
                            break;

                    }

                    break;

                case FormElementClass.Grid:

                    switch (m_subType)
                    {
                        case 0:
                            result = "Табличное поле";
                            break;
                        case 1:
                            result = "Таблица (Табличное поле)";
                            break;
                        case 2:
                            result = "Таблица (поле списка)";
                            break;
                        default:
                            result = "Grid_" + m_subType;
                            break;

                    }

                    break;

                default:

                    result = "Unknown_" + m_subType;
                    break;

            }

            return result;
        }

        private FormElementClass m_Class;
        private int m_subType;


        public static FormElementClass ParseTypeID(String TypeID)
        {
            switch (TypeID)
            {
                case "77ffcc29-7f2d-4223-b22f-19666e7250ba":
                    return FormElementClass.Field;
                case "cd5394d0-7dda-4b56-8927-93ccbe967a01":
                    return FormElementClass.Group;
                case "a9f3b1ac-f51b-431e-b102-55a69acdecad":
                    return FormElementClass.Button;
                case "3d3cb80c-508b-41fa-8a18-680cdf5f1712":
                    return FormElementClass.Label;
                case "143c00f7-a42d-4cd7-9189-88e4467dc768":
                    return FormElementClass.Grid;
                default:
                    return FormElementClass.Unknown;
            }

        }

    }
    
    #endregion

    #region Attributes

    class ManagedFormAttribute : IMDTreeItem
    {
        private ManagedFormAttribute(String name, String synonym, ManagedFormAttributeType type) : this (name, synonym, type, null)
        {
            
        }

        private ManagedFormAttribute(String name, String synonym, ManagedFormAttributeType type, ManagedFormAttribute[] children)
        {
            m_Type = type;
            Name = name;
            Title = synonym;

            if (children != null)
            {
                m_ChildItems = new MDObjectsCollection<ManagedFormAttribute>();

                foreach (var child in children)
                {
                    m_ChildItems.Add(child);
                }
            }
        }

        public bool IsBasic { get; private set; }
        public MDObjectsCollection<ManagedFormAttribute> ChildAttributes
        {
            get
            {
                return m_ChildItems;
            }
        }
        public ManagedFormAttributeType Type { get { return m_Type; } }

        ///////////////////////////
        // Private section ///////////////////////

        private ManagedFormAttributeType m_Type;
        private MDObjectsCollection<ManagedFormAttribute> m_ChildItems = null;

        public string Name  { get; private set; }
        public string Title { get; private set; }
        
        public static ManagedFormAttribute CreateFromList(SerializedList attrLst)
        {

            String Name = attrLst.Items[3].ToString();
            String Syn = null;
            if (attrLst.Items[4].Items[1].ToString() == "0")
            {
                Syn = Name;
            }
            else
            {
                Syn = attrLst.Items[4].Items[2].Items[1].ToString();
            }

            bool isBasic = attrLst.Items[10].ToString() == "1";

            var pattern = (SerializedList)attrLst.Items[5];

            ManagedFormAttributeType type = new ManagedFormAttributeType();
            type.TypeDescription = V8TypeDescription.ReadFromList(pattern);

            ManagedFormAttribute attr;
            
            int childCount = Int32.Parse(attrLst.Items[13].ToString());
            if (childCount > 0)
            {
                int baseIndex = 14;

                ManagedFormAttribute[] children = new ManagedFormAttribute[childCount];

                for (int i = 0; i < childCount; i++)
                {
                    SerializedList innerAttr = (SerializedList)attrLst.Items[baseIndex + i];

                    children[i] = CreateChildFromList(innerAttr);

                }

                attr = new ManagedFormAttribute(Name, Syn, type, children);

            }
            else
            {
                 attr = new ManagedFormAttribute(Name, Syn, type);
            }

            attr.IsBasic = isBasic;

            return attr;

        }

        private static ManagedFormAttribute CreateChildFromList(SerializedList childAttr)
        {
            String Name = childAttr.Items[3].ToString();
            String Syn = null;
            if (childAttr.Items[4].Items.Count < 3 || childAttr.Items[4].Items[2].ToString() == "0")
            {
                Syn = Name;
            }
            else
            {
                Syn = childAttr.Items[4].Items[2].Items[1].ToString();
            }

            var pattern = (SerializedList)childAttr.Items[5];

            ManagedFormAttributeType type = new ManagedFormAttributeType();
            type.TypeDescription = V8TypeDescription.ReadFromList(pattern);

            return new ManagedFormAttribute(Name, Syn, type);
        }

        ////////////////////////////////////////////////////
        // interfaces
        
        #region IMDTreeItem

        public string Key
        {
            get { return Name; }
        }

        public string Text
        {
            get { return Name; }
        }

        public AbstractImage Icon
        {
            get 
            {
                return IconCollections.ManagedForm["Attribute"];
            }
        }

        public bool HasChildren()
        {
            return m_ChildItems != null;
        }

        public IEnumerable<IMDTreeItem> ChildItems
        {
            get 
            {
                return m_ChildItems;
            }
        }

        #endregion
    }

    struct ManagedFormAttributeType
    {
        public V8TypeDescription TypeDescription { get; set; }
    }

    #endregion

}
