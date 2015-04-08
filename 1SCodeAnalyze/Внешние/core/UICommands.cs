using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace V8Reader.Core
{
    
    interface ICommandProvider
	{
        IEnumerable<UICommand> Commands { get; }
	}

    class UICommand
    {

        public UICommand(string Name, object srcObject, Action Callback)
        {
            Text = Name;
            m_Source = srcObject;
            m_LogicImpl = Callback;
        }

        public string Text { get; set; }

        public void Execute(Window parentWindow)
        {
            m_LogicImpl();
        }

        public override string ToString()
        {
            return Text;
        }

        private object m_Source;
        private Action m_LogicImpl;
    }

}
