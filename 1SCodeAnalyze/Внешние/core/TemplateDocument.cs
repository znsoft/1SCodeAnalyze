using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using V8Reader.Editors;

namespace V8Reader.Core
{
    
    abstract class TemplateDocument : IEditable, Comparison.IComparableItem
    {
        public TemplateDocument(MDTemplate OwnerTemplate)
        {
            m_Owner = OwnerTemplate;
        }

        public MDTemplate Owner
        {
            get
            {
                return m_Owner;
            }
            
        }

        protected IV8MetadataContainer Reader
        {
            get { return Owner.Container; }
        }

        public virtual ICustomEditor GetEditor()
        {
            throw new NotImplementedException();
        }

        private MDTemplate m_Owner;

        #region IComparableItem Members

        public abstract bool CompareTo(object Comparand);
        public abstract Comparison.IDiffViewer GetDifferenceViewer(object Comparand);
        
        #endregion
    }

}
