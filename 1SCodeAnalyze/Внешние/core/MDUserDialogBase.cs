using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace V8Reader.Core
{
    abstract class MDUserDialogBase
    {
        public MDUserDialogBase(SerializedList content)
        {
            m_Content = content;
        }

        public override string ToString()
        {
            return m_Content.ToString();
        }

        protected SerializedList m_Content;
    }

    class SimpleDialogStub : MDUserDialogBase
    {
        public SimpleDialogStub(SerializedList content) : base(content)
        {

        }
    }

}
