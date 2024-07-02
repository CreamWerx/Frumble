using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    public bool PerformAction(string actionOn, string action)
    {
        Log($"{actionOn} {action}");
        if (actionOn.Contains("File") && action == "Paste")
        {
            return true;
        }
        return false;
    }

    private void Log(string v)
    {
        Debug.WriteLine(v);
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

    private bool TreeViewSeekToItem(string path)
    {
        HistoryNavigation = true;
        var pathSplit = path.Split(System.IO.Path.DirectorySeparatorChar);
        if (pathSplit is null)
        {
            return false;
        }
        TViewItem? tmpTVI = GetTVIByHeader(tv, pathSplit[0]);
        if (tmpTVI is null)
        {
            return false;
        }
        UpdateTreeViewItem(tmpTVI);
        tmpTVI.IsExpanded = true;

        // Here we have the first NPTreeViewItem in TreeView
        bool success = true;
        for (int i = 1; i < pathSplit.Length; i++)
        {
            TViewItem? nPTreeViewItem = GetTVIByHeader(tmpTVI, pathSplit[i]);
            if (nPTreeViewItem is null)
            {
                Log("Searching tree failed");
                success = false;
                break;
            }

            // Here we have the next NPTreeViewItem in previous item,
            //  so update the listView, and expand.
            HistoryNavigation = true;
            UpdateTreeViewItem(nPTreeViewItem);
            HistoryNavigation = true;
            nPTreeViewItem.IsExpanded = true;
            tmpTVI = nPTreeViewItem;
        }

        // Perhaps GetTVIByHeader() failed
        if (success)
        {
            TViewItem targetItem = tmpTVI;

            // IsSelected will cause targetItem to be updated with TVMethods.UpdateTreeViewItem()
            //  via tree_SelectedItemChanged event
            HistoryNavigation = true;
            targetItem.IsSelected = true;

            // At this point we have the last element in the path and want to ensure it's visible.
            //  but we want as many of targetItem.Items as possible (if any) to be visible.
            // In my case there is space for ~24 items, so utilize them all if possivle,
            //  while ensuring targetItem is still in view (at the top if necessary).
            int count = targetItem.Items.Count;
            if (count > 0)
            {
                targetItem = (TViewItem)(targetItem.Items[(count < 22) ? count - 1 : 21]);
            }
            targetItem.BringIntoView();
            return true;
        }
        return false;
    }

    // Method overloaded tp accommodate first first call.
    private TViewItem? GetTVIByHeader(TreeView tmpTV, string headerName)
    {
        foreach (var item in tmpTV.Items)
        {
            var tvi = (TViewItem)item;
            if ((tvi.Header as string) == headerName)
            {
                return tvi;
            }
        }
        return null;
    }

    private TViewItem? GetTVIByHeader(TViewItem tmpTVI, string headerName)
    {
        foreach (var item in tmpTVI.Items)
        {
            var tvi = (TViewItem)item;
            if ((tvi.Header as string) == headerName)
            {
                return tvi;
            }
        }
        return null;
    }

    internal void UpdateTreeViewItem(TViewItem selectetItem)
    {
        string? path = selectetItem?.ItemPath;
        if (path != null)
        {
            try
            {
                selectetItem.Items.Clear();
                List<string> dirList = Directory.EnumerateDirectories(path, "*", new EnumerationOptions { IgnoreInaccessible = true }).ToList();
                foreach (var dir in dirList)
                {
                    var item = new TViewItem(dir);
                    selectetItem?.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }
    }
}
