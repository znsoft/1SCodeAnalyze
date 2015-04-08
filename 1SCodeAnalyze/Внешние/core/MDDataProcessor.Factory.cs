using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace V8Reader.Core
{
    partial class MDDataProcessor
    {
        public static MDDataProcessor Create(IV8MetadataContainer Container, SerializedList Content)
        {

            MDDataProcessor NewMDObject = new MDDataProcessor();

            NewMDObject.Container = Container;

            StaticMDIdentifiers ids  = new StaticMDIdentifiers();
            ids.AttributesCollection = "ec6bb5e5-b7a8-4d75-bec9-658107a699cf";
            ids.TablesCollection     = "2bcef0d1-0981-11d6-b9b8-0050bae0a95d";
            ids.FormsCollection      = "d5b0e5ed-256d-401c-9c36-f630cafd8a62";
            ids.TemplatesCollection  = "3daea016-69b7-4ed4-9453-127911372fe6";

            ReadFromStream(NewMDObject, Content, ids);

            return NewMDObject;
            
        }

    }
}
