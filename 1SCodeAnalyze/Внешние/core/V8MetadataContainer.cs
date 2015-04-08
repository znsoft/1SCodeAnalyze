using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace V8Reader.Core
{
    interface IV8MetadataContainer
    {
        string FileName { get; }
        MDFileItem GetElement(string ItemName);
        MDFileItem GetElement(string ItemName, System.IO.FileAccess Access);
    }
    
    class V8MetadataContainer : IDisposable, IV8MetadataContainer
    {
        const string kExtProccessing = "c3831ec8-d8d5-4f93-8a22-f9bfae07327f";
        const string kExtReport = "e41aff26-25cf-4bb6-b6c1-3f478a75f374";
        
        public V8MetadataContainer(string FileName)
        {
            _fileName = FileName;

            try
            {
                _reader = new MDReader(_fileName);
            }
            catch
            {
                _reader.Dispose();
                throw;
            }
        }

        public string FileName 
        { 
            get { return _fileName; } 
        }

        public MDObjectBase RaiseObject()
        {

            SerializedList procData = GetMainStream(_reader);

            string classID = procData.Items[3].Items[0].ToString();

            MDObjectBase newObj;
            switch (classID)
            {
                case kExtProccessing:
                    newObj = MDDataProcessor.Create(new NonDisposableContainer(this), procData);
                    break;
                case kExtReport:
                    newObj = MDReport.Create(new NonDisposableContainer(this), procData);
                    break;
                default:
                    throw new InvalidOperationException("Unknown container");
            }

            return newObj;

        }

        public MDFileItem GetElement(string ItemName)
        {
            return _reader.GetElement(ItemName);
        }

        public MDFileItem GetElement(string ItemName, System.IO.FileAccess Access)
        {
            if (Access == System.IO.FileAccess.Read)
            {
                return _reader.GetElement(ItemName);
            }
            else
            {
                throw new NotSupportedException("Write operations aren't supported");
            }
        }

        private SerializedList GetMainStream(MDReader Reader)
        {
            var Root = new SerializedList(Reader.GetElement("root").ReadAll());
            var TOCElement = Reader.GetElement(Root.Items[1].ToString());
            return new SerializedList(TOCElement.ReadAll());
        }

        private string _fileName;
        private MDReader _reader;

        
        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            _reader.Dispose();
            
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }

        }

        ~V8MetadataContainer()
        {
            Dispose(false);
        }

        #endregion

    }

    class NonDisposableContainer : IV8MetadataContainer
    {
        public NonDisposableContainer(V8MetadataContainer DisposableContainer)
        {
            _Container = DisposableContainer;
        }

        V8MetadataContainer _Container;

        public string FileName
        {
            get { return _Container.FileName; }
        }

        public MDFileItem GetElement(string ItemName)
        {
            return _Container.GetElement(ItemName);
        }

        public MDFileItem GetElement(string ItemName, System.IO.FileAccess Access)
        {
            return _Container.GetElement(ItemName, Access);
        }

    }

}
