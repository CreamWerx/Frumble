using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Frumble;
public class TViewItem : TreeViewItem
{
    public string ItemPath { get; set; }
    public string ItemName { get; set; }

    public TViewItem(string itemPath)
    {
        ItemPath = itemPath;
        ItemName = System.IO.Path.GetFileName(itemPath);

        // ItemPath could be the drive root (C:\\) therefore causing an issue
        if (string.IsNullOrWhiteSpace(ItemName))
        {
            ItemName = itemPath;
        }
        Header = ItemName;
        Background = Brushes.Black;
        ToolTip = ItemPath;
        Style = (Style)Application.Current.MainWindow.FindResource("PrimaryTVI");
    }
    
    public override string ToString()
    {
        return ItemName;
    }
}

public class LViewItem : ListViewItem
{
    public string ItemPath { get; set; }
    public string ItemName { get; set; }
    public int Bytes { get; set; } = -1;

    public LViewItem(string path)
    {
        ItemPath = path;
        ItemName = System.IO.Path.GetFileName(path);
        Content = ItemName;
        Background = Application.Current.MainWindow.Background;
        Foreground = Application.Current.MainWindow.Foreground;
        ToolTip = ItemPath;
        Style = (Style)Application.Current.MainWindow.FindResource("PrimaryLVI");
    }

    public LViewItem(string itemPath, string itemName)
    {
        ItemPath = itemPath;
        ItemName = itemName;
        Content = ItemName;
        Background = Application.Current.MainWindow.Background;
        Foreground = Application.Current.MainWindow.Foreground;
        ToolTip = ItemPath;
        Style = (Style)Application.Current.MainWindow.FindResource("PrimaryLVI");
    }

    public override string ToString()
    {
        return ItemPath;
    }
}
