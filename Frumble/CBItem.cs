using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Diagnostics;

namespace Frumble;
public class CBItem : CheckBox, IViewItem
{
    public event EventHandler? SelectAllChecked;
    public event EventHandler? SelectAllUnChecked;

    public bool? FileOpSuccess { get; set; } = null;
    public bool IsItemChecked { get; set; } = false;
    public string ItemName { get; set; }
    public string ItemPath { get; set; }
    public LViewItem ItemLVItem { get; set; }
    public TViewItem ItemTVItem { get; set; }

    public CBItem(LViewItem lvi)
    {
        Foreground = Brushes.Ivory;
        ItemLVItem = lvi;
        ItemName = lvi.ItemName;
        ItemPath = lvi.ItemPath;
        Content = ItemName;
        Checked += CBItem_Checked;
        Unchecked += CBItem_Unchecked;

    }

    public CBItem(TViewItem tvi)
    {
        Foreground = Brushes.Ivory;
        ItemTVItem = tvi;
        ItemName = tvi.ItemName;
        ItemPath = tvi.ItemPath;
        Content = ItemName;
        Checked += CBItem_Checked;
        Unchecked += CBItem_Unchecked;

    }

    public CBItem(string text)
    {
        Foreground = Brushes.Gold;
        ItemName = text;
        ItemPath = text;
        Content = ItemName;
        Checked += SelectAll_Checked;
        Unchecked += SelectAll_Unchecked;
    }

    private void SelectAll_Unchecked(object sender, RoutedEventArgs e)
    {
        SelectAllUnChecked?.Invoke(this, EventArgs.Empty);
    }

    private void SelectAll_Checked(object sender, RoutedEventArgs e)
    {
        SelectAllChecked?.Invoke(this, EventArgs.Empty);
    }

    private void CBItem_Unchecked(object sender, RoutedEventArgs e)
    {
        IsItemChecked = false;
        //Debug.WriteLine($"Do not Cut {ItemPath}");
        //ItemUnChecked?.Invoke(this, this);
    }

    private void CBItem_Checked(object sender, RoutedEventArgs e)
    {
        IsItemChecked = true;
        //Debug.WriteLine($"Cut {ItemPath}");
        //ItemChecked?.Invoke(this, this);
    }

    public override string ToString()
    {
        return ItemName;
    }
}
