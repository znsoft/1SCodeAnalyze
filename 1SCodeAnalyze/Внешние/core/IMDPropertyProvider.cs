using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using V8Reader.Comparison;

namespace V8Reader.Core
{
    interface IMDPropertyProvider
    {
        IDictionary<string, PropDef> Properties { get; }
        
        object GetValue(string Key);

    }

    class PropDef : Comparison.IComparableItem
    {

        public static PropDef Create(string key, string name, object value)
        {
            return new PropDef() { Key = key, Name = name, Value = value };
        }

        public static PropDef Create(string key, string name, object value, Comparison.IComparator comparer)
        {
            return new PropDef() { Key = key, Name = name, Value = value, Comparator = comparer };
        }

        public string Key
        {
            get { return m_Key; }
            set { m_Key = value; }
        }

        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }
        

        public object Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }

        public Comparison.IComparator Comparator
        {
            get { return m_Comparator; }
            set { m_Comparator = value; }
        }

        public IValueVisualizer ValueVisualizer 
        {
            get
            {
                if(m_Visualizer == null)
                {
                    m_Visualizer = new SimpleTextVisualizer(Value);
                }

                return m_Visualizer;
            }

            set
            {
                m_Visualizer = value;
            }
        }

        private string m_Key;
        private object m_Value;
        private string m_Name;
        private Comparison.IComparator m_Comparator;
        private IValueVisualizer m_Visualizer;

        #region IComparableItem Members

        public bool CompareTo(object Comparand)
        {
            if (m_Comparator == null)
            {
                if (Value is Comparison.IComparableItem)
                {
                    return ((Comparison.IComparableItem)Value).CompareTo(Comparand);
                }
                else
                {
                    m_Comparator = new Comparison.BasicComparator();
                }
            }

            return m_Comparator.CompareObjects(Value, Comparand);

        }

        public Comparison.IDiffViewer GetDifferenceViewer(object Comparand)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    class PropertyHolder : IMDPropertyProvider
    {

        public PropertyHolder()
        {
            m_Props = new Dictionary<string, PropDef>();
        }

        public PropertyHolder(IDictionary<string, PropDef> PropsToHold)
        {
            m_Props = (Dictionary<string,PropDef>)PropsToHold;
        }

        public void Add(PropDef PropertyDefinition)
        {
            m_Props.Add(PropertyDefinition.Key, PropertyDefinition);
        }

        public void Add(string key, string name, object value)
        {
            PropDef prop = PropDef.Create(key, name, value);
            Add(prop);
        }

        #region IMDPropertyProvider Members

        public IDictionary<string, PropDef> Properties
        {
            get 
            {
                return m_Props;
            }
        }

        public object GetValue(string Key)
        {
            var prop = m_Props[Key];
            return prop.Value;
        }

        #endregion

        private Dictionary<string, PropDef> m_Props;

    }
}
