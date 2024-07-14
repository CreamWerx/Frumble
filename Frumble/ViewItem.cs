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
    public event EventHandler<TViewItem>? DoubleClicked;
    public string ItemPath { get; set; }
    public string ItemName { get; set; }

    public TViewItem(bool isSpecial, string header)
    {
        ItemName = header;
        Header = header;
        ItemPath = "Special";
        Background = Brushes.Transparent;
        Foreground = Brushes.Gold;
        ToolTip = ItemPath;
        Style = (Style)Application.Current.MainWindow.FindResource("PrimaryTVI");
    }

    public TViewItem(string itemPath)
    {
        ItemPath = itemPath;
        ItemName = System.IO.Path.GetFileName(itemPath.TrimEnd(Path.DirectorySeparatorChar));

        // ItemPath could be the drive root (C:\\) therefore causing an issue
        if (string.IsNullOrWhiteSpace(ItemName))
        {
            ItemName = itemPath.TrimEnd(Path.DirectorySeparatorChar);
        }
        Header = ItemName.TrimEnd(Path.DirectorySeparatorChar);
        Background = Brushes.Transparent;
        Foreground = Brushes.Gold;
        ToolTip = ItemPath;
        Style = (Style)Application.Current.MainWindow.FindResource("PrimaryTVI");
    }

    protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
    {
        DoubleClicked?.Invoke(this, this);
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
        Background = Brushes.Transparent;
        Foreground = Application.Current.MainWindow.Foreground;
        ToolTip = ItemPath;
        Style = (Style)Application.Current.MainWindow.FindResource("PrimaryLVI");
    }

    public LViewItem(string itemPath, string itemName)
    {
        ItemPath = itemPath;
        ItemName = itemName;
        Content = ItemName;
        Background = Brushes.Transparent;
        Foreground = Application.Current.MainWindow.Foreground;
        ToolTip = ItemPath;
        Style = (Style)Application.Current.MainWindow.FindResource("PrimaryLVI");
    }

    public override string ToString()
    {
        return ItemPath;
    }
}
