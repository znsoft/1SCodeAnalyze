using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace V8Reader.Core
{
    class StaticTreeNode : IMDTreeItem
    {
        public StaticTreeNode(String Name) : this(Name, default(AbstractImage), null) { }
        public StaticTreeNode(String Name, AbstractImage Icon) : this(Name, Icon, null) { }
        public StaticTreeNode(String Name, AbstractImage Icon, IEnumerable<IMDTreeItem> UnderlyingCollection)
        {
            m_Name = Name;
            m_Icon = Icon;
            m_Children = UnderlyingCollection;

            if (UnderlyingCollection != null)
            {
                m_HasChildren = UnderlyingCollection.GetEnumerator().MoveNext();
            }
            else
            {
                m_HasChildren = false;
            }
        }

        public String Key
        {
            get { return null; }
        }

        public String Text
        {
            get { return m_Name; }
        }

        public AbstractImage Icon
        {
            get { return m_Icon; }
        }

        public bool HasChildren()
        {
            return m_HasChildren;
        }

        public IEnumerable<IMDTreeItem> ChildItems
        {
            get { return m_Children; }
        }

        private String m_Name;
        private AbstractImage m_Icon;
        private IEnumerable<IMDTreeItem> m_Children;
        private bool m_HasChildren;
    }
}
