using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Frumble;
public class MenuItemEx : MenuItem
{
    public event EventHandler<string>? Clicked;
    public string ItemName { get; set; }
    public string? ExePath { get; set; }
    public string ItemPath { get; set; }

    public MenuItemEx(string exePath)
    {
        ExePath = exePath;
        ItemName = Path.GetFileNameWithoutExtension(exePath);
        Header = ItemName;
        Background = Application.Current.MainWindow.Background;
        Foreground = Application.Current.MainWindow.Foreground;
    }

    public MenuItemEx(string itemPath, bool isNormal)
    {
        ItemPath = itemPath;
        ItemName = Path.GetFileName(itemPath);
        Header = ItemName;
        Background = Application.Current.MainWindow.Background;
        Foreground = Application.Current.MainWindow.Foreground;
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        e.Handled = true;
        Clicked?.Invoke(this, ExePath ?? ItemPath);
    }

    public override string ToString()
    {
        return ExePath;
    }
}
