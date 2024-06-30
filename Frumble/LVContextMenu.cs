using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Frumble;
public class LVContextMenu : ContextMenu
{

}

public class LVContextMenuItem : MenuItem
{
    public string? ItemName { get; set; }
    public string? ItemPath { get; set; }

    public LVContextMenuItem()
    {
        Header = "not set";
        SetBFGround();
    }

    public LVContextMenuItem(string exePath)
    {
        ItemPath = exePath;
        ItemName = Path.GetFileNameWithoutExtension(exePath);
        Header = ItemName;
        SetBFGround();
    }

    public LVContextMenuItem(string exePath, string itemName)
    {
        ItemPath = exePath;
        ItemName = itemName;
        Header = ItemName;
        SetBFGround();
    }


    private void SetBFGround()
    {
        Background = Application.Current.MainWindow.Background;
        Foreground = Application.Current.MainWindow.Foreground;
    }

    public override string ToString()
    {
        return ItemName ?? "not set";
    }
}
