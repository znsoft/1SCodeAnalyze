using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CFReader;

namespace V8Reader.Core
{
    class MDFileItem
    {
        public MDFileItem(CFReader.IImageLister imgLister, string itemName)
        {

            try
            {
                var handle = imgLister.GetItem(itemName);
                m_DataElement = handle.GetElement();
            }
            catch (V8ItemNotFoundException)
            {
                throw new FileNotFoundException();
            }

            if (m_DataElement is CFReader.IImageLister)
            {
                ElemType = ElementType.Directory;
            }
            else
            {
                ElemType = ElementType.File;
            }
            
        }

        public String Name 
        { 
            get { return m_DataElement.Name; } 
        }
        
        public ElementType ElemType { get; private set; }

        public enum ElementType
        {
            File,
            Directory
        }

        public Stream GetStream()
        {
            if (ElemType == ElementType.File)
            {
                return m_DataElement.GetDataStream();
            }
            else
            {
                throw new NotSupportedException();
            }

        }

        public String ReadAll()
        {
            if (ElemType == ElementType.File)
            {
                using (var reader = new StreamReader(GetStream()))
                {
                    return reader.ReadToEnd();
                }
            }
            else
            {
                throw new NotSupportedException();
            }

        }

        public MDFileItem GetElement(String itemName)
        {

            if (ElemType == ElementType.Directory)
            {

                var imgLister = (CFReader.IImageLister)m_DataElement;

                return new MDFileItem(imgLister, itemName);

            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private CFReader.V8DataElement m_DataElement;

    }
}
