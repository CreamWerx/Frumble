using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Frumble;
public static class CommonMethods
{
    public static void OpenWithDefaultApp(string fullPath)
    {
        try
        {
            var p = new Process
            {
                StartInfo = new ProcessStartInfo(fullPath)
                {
                    UseShellExecute = true
                }
            };
            p.Start();
        }
        catch (Exception ex)
        {
            Error(ex.Message);
        }
    }

    private static void Error(string message)
    {
        MessageBox.Show(message);
    }
}
