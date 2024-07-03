using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Frumble;
public static class Extensions
{
    public static void AppendText(this TextBlock block, string text)
    {
        block.Text += $"{DateTime.Now}: {text}{Environment.NewLine}{Environment.NewLine}";
    }
}
