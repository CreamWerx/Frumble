using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Frumble;
public class Logger
{
    public void Log(TextBox tmp, string msg)
    {
        //return;
        //if (Application.Current.Dispatcher.CheckAccess())
        //{
        //    // do whatever you want to do with shared object.
        //    tmp.Text = msg;
        //}
        //else
        //{
        //    //Other wise re-invoke the method with UI thread access
        //    Application.Current.Dispatcher.Invoke(new System.Action(() => Log(msg)));
        //}
        //tblLog.AppendText(msg);
        tmp.Dispatcher.BeginInvoke(()=> tmp.Text = msg);
    }
}
