using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace V8Reader.Core
{
    class HTMLTemplate : FWOpenableDocument
    {
        public HTMLTemplate(MDTemplate OwnerTemplate) : base (OwnerTemplate)
        {
            try
            {
                var fileElem = Reader.GetElement(GetFileName());
                SerializedList content = new SerializedList(fileElem.ReadAll());
                m_HTMLDoc = new HTMLDocument(content);

            }
            catch (System.IO.FileNotFoundException exc)
            {
                throw new MDObjectIsEmpty(OwnerTemplate.Name, exc);
            }
        }

        public override string Extract()
        {
            return m_HTMLDoc.Location;
        }

        private HTMLDocument m_HTMLDoc;
    }
}
