using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;

namespace V8Reader.Core
{
    class V8ModuleProcessor : Editors.IEditable, Comparison.IComparableItem
    {
        public V8ModuleProcessor(string text) : this(text, "")
        {
            
        }

        public V8ModuleProcessor(string text, string moduleName)
        {
            Text = text;
            ModuleName = moduleName;
        }

        private string m_Text;

        public string Text
        {
            get { return m_Text; }
            set { m_Text = value; }
        }

        public string ModuleName { get; set; }

        #region IEditable Members

        public Editors.ICustomEditor GetEditor()
        {
            return new Editors.ModuleEditor(this, true);
        }

        #endregion

        #region IComparableItem Members

        public bool CompareTo(object Comparand)
        {
            if(Comparand is String)
            {
                return Text == (string)Comparand;
            }
            else if (Comparand is V8ModuleProcessor)
            {
                return Text == ((V8ModuleProcessor)Comparand).Text;
            }
            else
            {
                return this.Equals(Comparand);
            }
        }

        #endregion

        public Comparison.IDiffViewer GetDifferenceViewer(object Comparand)
        {
            if (Comparand is String)
            {
                return new Comparison.ExternalTextDiffViewer(Text, (string)Comparand);
            }
            else if (Comparand is V8ModuleProcessor)
            {
                return new Comparison.ExternalTextDiffViewer(Text, ((V8ModuleProcessor)Comparand).Text);
            }
            else
            {
                throw new ArgumentException();
            }
        }

    }

}
