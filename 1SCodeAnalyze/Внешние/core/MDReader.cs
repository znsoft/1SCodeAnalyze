using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace V8Reader.Core
{

    class MDReader : IDisposable
    {
        public MDReader(String File)
        {
            m_FileReader = new CFReader.V8File(File);
        }

        public MDFileItem GetElement(String Name)
        {
            if (IsDisposed())
                throw new ObjectDisposedException(GetType().ToString());

            return new MDFileItem(m_FileReader.GetLister(), Name);

        }

        public bool IsDisposed()
        {
            return m_FileReader == null;
        }

        public void Dispose()
        {
            DisposeImpl(true);
        }

        private void DisposeImpl(bool ManualDisposal)
        {
            if (!IsDisposed())
            {
                m_FileReader.Dispose();

                if (ManualDisposal)
                    GC.SuppressFinalize(this);

                m_FileReader = null;
            }
        }

        ~MDReader()
        {
            DisposeImpl(false);
        }

        private CFReader.V8File m_FileReader;

    }
}
