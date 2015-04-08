using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace V8Reader.Core
{
    interface IMDTreeItem
    {

        String Key { get; }
        String Text { get; }
        AbstractImage Icon { get; }
        
        bool HasChildren();
        IEnumerable<IMDTreeItem> ChildItems { get; }

    }

}
