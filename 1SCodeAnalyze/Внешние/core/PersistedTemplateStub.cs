using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace V8Reader.Core
{
    class PersistedTemplateStub : FWOpenableDocument
    {
        public PersistedTemplateStub(MDTemplate OwnerTemplate) : base(OwnerTemplate) { }
    }
}
