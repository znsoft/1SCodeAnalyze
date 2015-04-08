using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace V8Reader.Core
{
    internal class DCSSchemaDocument : FWOpenableDocument
    {
        public DCSSchemaDocument(MDTemplate OwnerTemplate) : base(OwnerTemplate)
        {
        }

        private bool _isLoaded;
        private string _SchemaContent;

        public string SchemaContent 
        { 
            get
            {
                LoadDataIfNeeded();
                return _SchemaContent;
            }
        }

        private void LoadDataIfNeeded()
        {
            if (!_isLoaded)
            {
                LoadData();
            }
        }

        public override string Extract()
        {
            string Filename = DefaultExtractionPath();
            using (var writer = new System.IO.StreamWriter(Filename, false, Encoding.UTF8))
            {
                writer.Write(SchemaContent);
            }

            return Filename;
        }

        protected override bool InternalCompare(FWOpenableDocument cmpDoc)
        {
            if (cmpDoc is DCSSchemaDocument)
            {
                return SchemaContent == ((DCSSchemaDocument)cmpDoc).SchemaContent;
            }
            else
            {
                return false;
            }
        }

        private void LoadData()
        {
            
            using (var src = GetDataStream())
            {
                using (var rdr = new System.IO.BinaryReader(src))
                {
                    int marker = rdr.ReadInt32();
                    if (marker == 0)
                    {
                        ReadSchemaFiles(rdr);
                    }
                    else
                    {
                        rdr.BaseStream.Position = 0;
                        ReadPlainXML(rdr.BaseStream);
                    }

                    _isLoaded = true;
                }
            }


        }

        private void ReadPlainXML(System.IO.Stream stream)
        {
            _SchemaContent = ReadStreamAsText(stream);
        }

        private void ReadSchemaFiles(System.IO.BinaryReader rdr)
        {
            int variantNum = rdr.ReadInt32();
            Int64 SchemaLen = rdr.ReadInt64();

            Int64[] lenArray = new Int64[variantNum];
            for (int i = 0; i < variantNum; i++)
            {
                lenArray[i] = rdr.ReadInt64();
            }

            // вряд ли кто-то засунет в схему данные не влезающие в Int32
            // поэтому, не будем заморачиваться с длиной в Int64 
            // (BinaryReader.ReadBytes не работает c Int64, читать частями лень)

            var tmp_SchemaContent = ReadUTF8Array(rdr.ReadBytes((Int32)SchemaLen));
            var settingsList = new List<string>();

            for (int i = 0; i < variantNum; i++)
            {
                settingsList.Add(ReadUTF8Array(rdr.ReadBytes((Int32)lenArray[i])));
            }

            JoinSchemaAndVariants(tmp_SchemaContent, settingsList);
        }

        private void JoinSchemaAndVariants(string tmp_SchemaContent, List<string> settingsList)
        {
            XNamespace schemaNS = XNamespace.Get("http://v8.1c.ru/8.1/data-composition-system/schema");
            XNamespace settingsNS = XNamespace.Get("http://v8.1c.ru/8.1/data-composition-system/settings");

            Utils.XMLMerge.NamespaceMap nsMap = new Utils.XMLMerge.NamespaceMap();
            nsMap.Add("dcsset", settingsNS.NamespaceName);
            nsMap.Add("dcscor", "http://v8.1c.ru/8.1/data-composition-system/core");
            nsMap.Add("xs"    , "http://www.w3.org/2001/XMLSchema");
            nsMap.Add("xsi"   , "http://www.w3.org/2001/XMLSchema-instance");

            XDocument File = XDocument.Parse(tmp_SchemaContent);
            XContainer schema = File.Root.Element(XName.Get("dataCompositionSchema", schemaNS.NamespaceName));

            if (schema == null)
            {
                _SchemaContent = tmp_SchemaContent;
                return;
            }

            if (settingsList.Count == 0)
            {
                _SchemaContent = schema.ToString();
                return;
            }

            var elemVariants = schema.Elements(XName.Get("settingsVariant", schemaNS.NamespaceName));
            if (elemVariants == null)
            {
                // непонятная ситуация, в схеме нет описания вариантов, но сами варианты есть.
                // пока оставим просто схему, потом видно будет
                _SchemaContent = schema.ToString();
                return;
            }

            int i = 0;
            foreach (var variant in elemVariants)
            {
                if (i >= settingsList.Count)
                {
                    break;
                }

                var settings = XDocument.Parse(settingsList[i++]);
                
                if (settings.Root.GetDefaultNamespace() == settingsNS)
                {
                    XName currentName = XName.Get("settings",settingsNS.NamespaceName);
                    settings.Root.Name = currentName;

                    Utils.XMLMerge.Perform(variant, settings.Root, nsMap);
                    
                    //variant.Add(settings.Root);
                }

            }

            _SchemaContent = schema.ToString();
        }

        private static string ReadUTF8Array(byte[] arr)
        {
            string result;

            using (var ms = new System.IO.MemoryStream(arr))
            {
                result = ReadStreamAsText(ms);
            }

            return result;
        }

        private static string ReadStreamAsText(System.IO.Stream Stream)
        {
            string result;

            using (var sr = new System.IO.StreamReader(Stream, true))
            {
                result = sr.ReadToEnd();
            }

            return result;
        }

    }
}
