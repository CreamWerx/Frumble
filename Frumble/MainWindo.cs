using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Frumble;
public partial class MainWindow
{
    private (bool success, int count) UpdateTViewItem(TViewItem tViewItem)
    {
        tViewItem.Items.Clear();
        try
        {
            var directories = Directory.EnumerateDirectories(tViewItem.ItemPath, "*", new EnumerationOptions { IgnoreInaccessible = true });
            foreach (var dir in directories)
            {
                var tvi = new TViewItem(dir);
                tvi.MouseDoubleClick += TViewItem_MouseDoubleClick;
                tViewItem.Items.Add(tvi);
            }
            return (true, directories.Count());
        }
        catch (UnauthorizedAccessException)
        {
            tViewItem.ToolTip = new string("locked");
            tViewItem.Foreground = Brushes.Red;
            return (false, 0);
        }
        catch (Exception ex)
        {
            if (ex.Message.StartsWith("This drive is locked by BitLocker"))
            {
                tViewItem.Foreground = Brushes.Red;
                tViewItem.ToolTip = "BitLocker";
                return (false, 0);
            }
            Error(ex.Message);
            return (false, 0);
        }
    }

    public (bool success, int count) PopulateListView(string dirPath, ListView listView)
    {
        try
        {
            listView.Items.Clear();
            var files = Directory.EnumerateFiles(dirPath, "*.*", new EnumerationOptions { IgnoreInaccessible = true });
            foreach (var file in files)
            {
                var lViewItem = new LViewItem(file);
                lViewItem.MouseDoubleClick += LViewItem_MouseDoubleClick;
                lViewItem.MouseEnter += LViewItem_MouseEnter;
                listView.Items.Add(lViewItem);
            }
            return (true, files.Count());
        }
        catch (Exception ex)
        {
            if (ex.Message.StartsWith("This drive is locked by BitLocker"))
            {
                var selectedTVItem = (TViewItem)tv.SelectedItem;
                selectedTVItem.Foreground = Brushes.Red;
                selectedTVItem.IsEnabled = false;
                return (false, 0);
            }
            Error(ex.Message);
            return (false, 0);
        }
    }

    public bool ListDrives(TreeView tree)
    {
        try
        {
            var drives = Directory.GetLogicalDrives();
            foreach (var drive in drives)
            {
                TViewItem tViewItem = new TViewItem(drive);
                tViewItem.Foreground = Brushes.Ivory;
                tViewItem.MouseDoubleClick += TViewItem_MouseDoubleClick;
                tree.Dispatcher.Invoke(() => tree.Items.Add(tViewItem));
            }
            return true;
        }
        catch (Exception ex)
        {
            Error(ex.Message);
            return false;
        }
    }

    private void Error(string message)
    {
        MessageBox.Show(message);
    }

    private void BuildLVContextMenu()
    {
        try
        {
            var paths = File.ReadAllLines(openWithPath.Text);
            foreach (var item in paths)
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    MenuItemEx menuItem = CreateOpenWithMenuItem(item);
                    openWith.Items.Add(menuItem);
                }
            }
        }
        catch (Exception ex)
        {
            Error(ex.Message);
        }
    }

    private (bool success, int count) SearchCurrentDirectory(string text)
    {
        MessageBox.Show("NotImplementedException");
        return (false, 0);
    }

    private void DisableRedundantMenuItems()
    {
        if (SelectedLVItems?.Count < 1) { openWith.IsEnabled = false; return; } else openWith.IsEnabled = true;
        if (!((LViewItem)lv.SelectedItem).ItemPath.EndsWith(".exe")) addThisApp.IsEnabled = false; else addThisApp.IsEnabled = true;
    }

    private MenuItemEx CreateOpenWithMenuItem(string exePath)
    {
        var menuItem = new MenuItemEx(exePath);
        menuItem.Clicked += LVMenuItem_Clicked;

        return menuItem;
    }

    private void SetFooter(int count)
    {
        string collective = (count == 1) ? "item" : "items";
        footer.Text = $"{count} {collective} selected";
    }
}
