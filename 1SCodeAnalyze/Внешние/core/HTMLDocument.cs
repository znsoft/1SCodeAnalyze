using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace V8Reader.Core
{
    sealed class HTMLDocument : IDisposable, Comparison.IComparableItem
    {

        public HTMLDocument()
        {
            IsEmpty = true;
            m_Extracted = true;
        }

        public HTMLDocument(SerializedList lstContent)
        {
            m_Content = lstContent;
            m_Extracted = false;
            m_Disposed = false;
        }

        public String Location
        {
            get 
            {
                if (!m_Extracted)
                    Extract();
                
                if (IsEmpty)
                    return String.Empty;

                return DocumentName();

            }
        }

        public bool IsEmpty 
        {
            get
            {
                if (m_Extracted)
                    return m_IsEmpty;

                try
                {
                    int count = Int32.Parse(m_Content.Items[1].ToString());
                    if (count == 0)
                    {
                        m_IsEmpty = true;
                    }
                }
                catch
                {
                    m_Extracted = true;
                    throw;
                }

                return m_IsEmpty;

            }
            private set
            {
                m_IsEmpty = value;
            }
        }

        private void Extract()
        {
            if (m_Extracted)
                return;
            if (m_Disposed)
                throw new ObjectDisposedException(this.ToString());

            if (IsEmpty)
            {
                m_Extracted = true;
                return;
            }

            m_WorkDir = Path.GetTempPath() + Guid.NewGuid().ToString() + "\\";

            Directory.CreateDirectory(m_WorkDir);

            ExtractHTML();
            ExtractImages();

            m_Extracted = true;
            
        }

        private void ExtractHTML()
        {
            var FileName = DocumentName();
            if (!File.Exists(FileName))
            {
                WriteBase64(FileName, m_Content.Items[3].Items[0].ToString());
            }
        }

        private void ExtractImages()
        {
            const int Base = 4;
            const int len = 3;
            const String Prefix = "038b5c85-fb1c-4082-9c4c-e69f8928bf3a_files";

            Directory.CreateDirectory(m_WorkDir + "\\" + Prefix);

            int imgCount = Int32.Parse(m_Content.Items[Base].ToString());

            if (imgCount > 0)
            {
                for (int i = 0; i < imgCount; ++i)
                {
                    var ImgName = Prefix + "\\" + SerializedList.StripQuotes(m_Content.Items[Base + 1 + (i*len)].ToString());
                    var Base64 = m_Content.Items[Base + 3 + (i * len)].Items[0].ToString();

                    WriteBase64(m_WorkDir + ImgName, Base64);
                    

                }
            }

        }

        private void WriteBase64(String FileName, String Base64)
        {
            using (FileStream writer = new FileStream(FileName, FileMode.OpenOrCreate))
            {
                StringBuilder reader = new StringBuilder(Base64);
                reader.Remove(0, 8);

                Byte[] byteArr = System.Convert.FromBase64String(reader.ToString());

                writer.Write(byteArr, 0, byteArr.Length);
            }
        }

        private String DocumentName()
        {
            return m_WorkDir + htmlName;
        }

        private bool m_Extracted;
        private bool m_Disposed;
        private bool m_IsEmpty;
        private String m_WorkDir;
        private SerializedList m_Content;
        private const String htmlName = "thisdoc.html";

        #region Cleanup code

        public void Dispose()
        {
            CleanUp();
            GC.SuppressFinalize(this);
        }

        ~HTMLDocument()
        {
            CleanUp();
        }

        public void CleanUp()
        {
            if (Directory.Exists(m_WorkDir))
            {
                try
                {
                    Directory.Delete(m_WorkDir, true);
                }
                catch (Exception)
                {
                    
                }
            }
            m_Disposed = true;
        }

        #endregion

        #region IComparableItem Members

        public bool CompareTo(object Comparand)
        {
            HTMLDocument htmlComparand = (HTMLDocument)Comparand;

            if(htmlComparand.m_Content != null)
            {
                if(m_Content == null)
                {
                    return false;
                }

                var comparator = new Comparison.BasicComparator();
                return comparator.CompareObjects(m_Content.ToString(), htmlComparand.m_Content.ToString());

            }
            else
            {
                return m_Content == null;
            }
        }

        public Comparison.IDiffViewer GetDifferenceViewer(object Comparand)
        {
            var DiffViewer = new Comparison.ExternalTextDiffViewer(m_Content.ToString(), ((HTMLDocument)Comparand).m_Content.ToString());
            return DiffViewer;
        }

        #endregion
    }
}
