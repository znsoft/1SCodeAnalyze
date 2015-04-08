using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace V8Reader.Core
{
    class SerializedItem
    {
        public SerializedItem(String Content)
        {
            RawContent = Content;
        }

        public override string ToString()
        {
            return RawContent;
        }

        public virtual List<SerializedItem> Items
        {
            get
            {
                return null;
            }
        }

        protected String RawContent;

    }
}
