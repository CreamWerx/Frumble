using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Frumble;
public partial class MainWindow
{

    private bool AddFrequent()
    {
        try
        {
            TViewItem tvi = ((TViewItem)tv.SelectedItem);
            if (tvi is not null)
            {
                File.AppendAllText(tbFrequentPath.Text, $"{tvi.ItemPath}{Environment.NewLine}");
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            //Log(ex.Message);
            return false;
        }
    }

    private void BuildLVContextMenu()
    {
        try
        {
            var openWithPaths = File.ReadAllLines(openWithPath.Text);
            foreach (var item in openWithPaths)
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
            //Log(ex.Message);
        }

        try
        {
            var sendToPaths = File.ReadAllLines(tbSendToPath.Text);
            foreach (var item in sendToPaths)
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    MenuItemEx menuItem = CreateSendToMenuItem(item);
                    sendTo.Items.Add(menuItem);
                }
            }
        }
        catch (Exception ex)
        {
            //Log(ex.Message);
        }
    }

    void CollapseTreeviewItems(TreeViewItem Item)
    {
        Item.IsExpanded = false;

        foreach (TreeViewItem item in Item.Items)
        {
            item.IsExpanded = false;

            //if (item.HasItems)
            //    CollapseTreeviewItems(item);
        }
    }

    private bool ConfirmCopy(string newFilePath)
    {
        return File.Exists(newFilePath);
    }

    private void ControlSuccess(LViewItem control, bool success = true)
    {
        //LViewItem lvi = (LViewItem)control;
        control.IsSelected = false;
        Color fadeColor = success ? Colors.LightGreen : Colors.Red;
        ColorAnimation ca = new ColorAnimation(Colors.Transparent, new Duration(TimeSpan.FromSeconds(1)));
        control.Background = new SolidColorBrush(fadeColor);
        control.Background.BeginAnimation(SolidColorBrush.ColorProperty, ca);
        //lvi.IsSelected = true;
    }

    private MenuItemEx CreateSendToMenuItem(string itemPath)
    {
        var menuItem = new MenuItemEx(itemPath, true);
        menuItem.Clicked += LVSendToMenuItem_Clicked;

        return menuItem;
    }
    private MenuItemEx CreateOpenWithMenuItem(string exePath)
    {
        var menuItem = new MenuItemEx(exePath);
        menuItem.Clicked += LVOpenWithMenuItem_Clicked;

        return menuItem;
    }

    private void DisableRedundantMenuItems()
    {
        if (SelectedLVItems?.Count < 1) { openWith.IsEnabled = false; return; } else openWith.IsEnabled = true;
        if (!((LViewItem)lv.SelectedItem).ItemPath.EndsWith(".exe")) addThisApp.IsEnabled = false; else addThisApp.IsEnabled = true;
    }

    private void Error(string message)
    {
        MessageBox.Show(message);
    }

    private bool FileOperation(string action)
    {
        switch (action)
        {
            case "Copy":
                return FileOperationCopy().Result;
            //return true;
            case "Delete":
                return FileOperationDelete();
        }
        return false;
    }

    private bool FileOperationDelete()
    {
        return false;
    }

    private async Task<bool> FileOperationCopy()
    {
        var copyItem = (LViewItem)lv.SelectedItem;
        //ControlSuccess(copyItem, true);
        //return true;
        string? dir = Path.GetDirectoryName(copyItem.ItemPath);
        if (string.IsNullOrWhiteSpace(dir))
        {
            ControlSuccess(copyItem, false);
            return false;
        }
        string newName = GetNewName(copyItem.ItemName, "_copy", true);
        string newPath = Path.Combine(dir, newName);
        //Log($"copy: {copyItem.ItemPath} to {newPath}");
        Task<bool>? res = null;
        //bool res = false;
        try
        {
            //var cpi = copyItem;
            //var old = copyItem.ItemName;
            res = Task.Run(() => SendTo(copyItem, newPath, $"copy: {copyItem.ItemName} to {newPath}"));
            if (res.Result)
            {
                ControlSuccess(copyItem, true);
                return true;
            }
        }
        catch (Exception ex)
        {
            Log(ex.Message);
            ControlSuccess(copyItem, false);
            return false;
        }
        //var success = SendTo(copyItem, newPath, $"copy: {copyItem.ItemName} to {newPath}");
        //copyItem.IsSelected = false;
        ControlSuccess(copyItem, false);
        //copyItem.IsSelected = true;

        return false;
    }

    private bool Frequent(string action)
    {
        switch (action)
        {
            case "Add":
                return AddFrequent();
        }
        return false;
    }

    private string GetNewName(string itemName, string newName, bool appendMode)
    {
        string name = Path.GetFileNameWithoutExtension(itemName);
        string extension = Path.GetExtension(itemName);
        if (appendMode)
        {
            name += newName + extension;
            return name;
        }
        return newName + extension;
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

    public TViewItem HistoryBack()
    {
        HistoryNavigation = true;
        currentHistoryPos--;
        btnBack.IsEnabled = (currentHistoryPos > 0) ? true : false;
        btnForward.IsEnabled = (currentHistoryPos <= (History.Count - 1)) ? true : false;
        if (currentHistoryPos == -1)
        {
            currentHistoryPos = 0;
        }
        return History[currentHistoryPos];
    }

    public TViewItem HistoryForward()
    {
        HistoryNavigation = true;
        currentHistoryPos++;
        btnBack.IsEnabled = (currentHistoryPos > 0) ? true : false;
        btnForward.IsEnabled = (currentHistoryPos < (History.Count - 1)) ? true : false;
        return History[currentHistoryPos];
    }

    private void ItemSuccess(LViewItem selectedItem, bool success)
    {
        Color fadeColor = success ? Colors.LightGreen : Colors.Red;
        //ColorAnimation ca = new ColorAnimation(Colors.Transparent, new Duration(TimeSpan.FromSeconds(1)));
        Dispatcher.Invoke(() => selectedItem.Background = new SolidColorBrush(fadeColor));
        Dispatcher.Invoke(() =>
        {
            selectedItem.Background.BeginAnimation(SolidColorBrush.ColorProperty,
            new ColorAnimation(Colors.Transparent,
            new Duration(TimeSpan.FromSeconds(2))));
        });
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
                //tViewItem.MouseDoubleClick += TViewItem_MouseDoubleClick;
                tViewItem.DoubleClicked += TViewItem_DoubleClicked;
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

    public void Log(string msg)
    {
        if (Dispatcher.CheckAccess())
        {
            tblLog.AppendText(msg);
        }
        else
        {
            Dispatcher.BeginInvoke(() => Log(msg));
            //Dispatcher.BeginInvoke(() => Log($"no access - {msg}"));
        }
    }

    public bool PerformAction(string actionOn, string action)
    {

        //Log($"PerformAction: {actionOn} - {action}");
        //return true;
        switch (actionOn)
        {
            case "Frequent":
                return Frequent(action);
            case "File Operations":
                return FileOperation(action);
        }
        return false;
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
                //lViewItem.MouseDoubleClick += LViewItem_MouseDoubleClick;
                lViewItem.PreviewMouseLeftButtonDown += LViewItem_PreviewMouseLeftButtonDown;
                lViewItem.MouseEnter += LViewItem_MouseEnter;
                //lViewItem.MouseLeftButtonDown += LViewItem_MouseLeftButtonDown;
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

    private void ScrollTviewItemsIntoView(TViewItem selectetItem)
    {
        //selectetItem.Focus();
        //return;
        int itemCount = selectetItem?.Items?.Count ?? 0;
        if (selectetItem is null || itemCount <= 0)
        {
            return;
        }
        int targetItemIndex = 21;
        if (itemCount < 22)
        {
            targetItemIndex = selectetItem.Items.Count - 1;
        }
        ((TViewItem)selectetItem.Items[targetItemIndex]).BringIntoView();
    }

    private (bool success, int count) SearchCurrentDirectory(string text)
    {
        MessageBox.Show("NotImplementedException");
        return (false, 0);
    }

    private bool SendTo(LViewItem selectedItem, string newFilePath, string tmp, bool overwrite = false)
    {
        Log(tmp);
        if (File.Exists(newFilePath))
        {
            Log("File Already exist");
            try
            {
                if (!overwrite)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                return false;
            }
        }
        try
        {
            File.Copy(selectedItem.ItemPath, newFilePath, overwrite);
            bool success = ConfirmCopy(newFilePath);
            //Log(tmp);
            if (success)
            {
                //Log("Success");
                //ItemSuccess(selectedItem, true);
                return true;
            }
            //Log("Failed");
            //ItemSuccess(selectedItem, false);
            return false;
        }
        catch (Exception ex)
        {
            //Log($"Failed: {ex.Message}");
            //ItemSuccess(selectedItem, false);
            return false;
        }
    }

    private void SetFooter(int count)
    {
        string collective = (count == 1) ? "item" : "items";
        footer.Text = $"{count} {collective} selected";
    }

    private void ToggleBreadCrumb(Visibility textVisible)
    {
        tbCurrentPath.Visibility = textVisible;
        //crumbPanel.Visibility = Visibility.Visible;
        Debug.WriteLine("ToggleBreadCrumb");
        crumbPanel.Children.Clear();
        foreach (CrumbItem cItem in crumbList)
        {
            var crumbBox = new ComboBox { IsEditable = true, IsReadOnly = true, Text = cItem.ItemName, Margin = new Thickness(10,0,0,0) };
            crumbBox.SelectionChanged += CrumbBox_SelectionChanged;
            crumbBox.MouseLeftButtonUp += CrumbBox_MouseLeftButtonUp;
            crumbBox.Tag = cItem.ItemPath;
            crumbBox.Style = (Style)FindResource("CrumbBox");
            foreach(SubItem viewItem in cItem.ContainedItems)
            {
                crumbBox.Items.Add(viewItem);
            }
            
            crumbPanel.Children.Add(crumbBox);
        }
        crumbSV.Visibility = (tbCurrentPath.Visibility == Visibility.Visible) ? Visibility.Hidden: Visibility.Visible;
    }

    private void CrumbBox_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        e.Handled = true;
        Log("CrumbBox_MouseLeftButtonUp");
        var cmbo = (ComboBox)sender;
        var newPath = (string)cmbo.Tag;
        //var newPath = item.ItemPath;
        TreeViewSeekToItem(newPath);
        ToggleBreadCrumb(Visibility.Hidden);
    }

    private void CrumbBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Log("CrumbBox_SelectionChanged");
        var cmbo = (ComboBox)sender;
        var item = (SubItem)cmbo.SelectedItem;
        cmbo.Tag = item.ItemPath;
        
        var newPath = (string)cmbo.Tag;
        TreeViewSeekToItem(newPath);
        ToggleBreadCrumb(Visibility.Hidden);
    }

    private bool TreeViewSeekToItem(string path, bool isHistoryNavigation = true)
    {
        //Seeking = true;
        HistoryNavigation = isHistoryNavigation;
        var pathSplit = path.Split(Path.DirectorySeparatorChar);
        if (pathSplit is null)
        {
            return false;
        }
        TViewItem? rootTVI = GetTVIByHeader(tv, pathSplit[0]);
        if (rootTVI is null)
        {
            return false;
        }
        //UpdateTreeViewItem(rootTVI);
        rootTVI.IsExpanded = true;

        // Here we have the first NPTreeViewItem in TreeView
        // Clear list and add a CrumbItem
        crumbList.Clear();
        crumbList.Add(new CrumbItem(rootTVI));
        bool success = true;
        for (int i = 1; i < pathSplit.Length; i++)
        {
            TViewItem? treeViewItem = GetTVIByHeader(rootTVI, pathSplit[i]);
            if (treeViewItem is null)
            {
                //Log("Searching tree failed");
                success = false;
                break;
            }

            // Here we have the next TViewItem in previous item,
            //  so update the TreeView, and expand.
            HistoryNavigation = true;
            //treeViewItem = UpdateTreeViewItem(treeViewItem);
            HistoryNavigation = true;
            treeViewItem.IsExpanded = true;
            // Add new crumb to list
            crumbList.Add(new CrumbItem(treeViewItem));
            rootTVI = treeViewItem;
        }

        // Perhaps GetTVIByHeader() failed
        if (success)
        {
            TViewItem targetItem = rootTVI;

            // IsSelected will cause targetItem to be updated with UpdateTreeViewItem()
            //  via tv_SelectedItemChanged event
            HistoryNavigation = true;
            targetItem.IsSelected = true;

            // At this point we have the last element in the path and want to ensure it's visible.
            //  but we want as many of targetItem.Items as possible (if any) to be visible.
            // In my case there is space for ~22 items, so utilize them all if possivle,
            //  while ensuring targetItem is still in view (at the top if necessary).
            int count = targetItem.Items.Count;
            //int tvHeight = (int)tv.ActualHeight;
            //int tvItemHeight = (int)((TViewItem)tv.Items[0]).ActualHeight;
            //int maxVisibleItems = tvHeight / tvItemHeight;
            //targetItem.Focus();
            if (count > 0)
            {
                targetItem = (TViewItem)(targetItem.Items[(count < 22) ? count - 1 : 21]);
            }
            targetItem.BringIntoView();
            return true;
        }
        return false;
    }

    private void UpdateCrumbs(string text)
    {
        CommonMethods.Bingo("NotImplementedException", true);
    }

    internal TViewItem? UpdateTreeViewItem(TViewItem selectetItem)
    {
        string? path = selectetItem?.ItemPath;
        if (path != null)
        {
            try
            {
                selectetItem?.Items.Clear();
                List<string> dirList = Directory.EnumerateDirectories(path, "*", new EnumerationOptions { IgnoreInaccessible = true }).ToList();
                foreach (var dir in dirList)
                {
                    var item = new TViewItem(dir);
                    selectetItem?.Items.Add(item);
                }
                ScrollTviewItemsIntoView(selectetItem);
                return selectetItem;
            }
            catch (Exception ex)
            {

                if (ex.Message.StartsWith("This drive is locked by BitLocker"))
                {
                    Log(ex.Message);
                    int startPos = ex.Message.IndexOf(":\\") - 1;
                    int endPas = ex.Message.Length - startPos - 1;
                    string driveLetter = ex.Message.Substring(startPos, endPas);
                    Log(driveLetter);
                    CommonMethods.OpenWith(@"C:\Windows\system32\bdeunlock.exe", driveLetter);
                    //var bitlocker = new BitLocker("localhost");
                    //var result = bitlocker.UnlockWithPassphrase("E:", "seagate1q2W3e4R5t");
                    //tViewItem.Foreground = Brushes.Red;
                    //tViewItem.ToolTip = "BitLocker";
                    return (null);
                }

                Log(ex.Message);
                return null;
            }
        }
        return null;
    }

    

    private (bool success, TViewItem? crumb) UpdateTViewItem(TViewItem tViewItem)
    {

        tViewItem.Items.Clear();
        try
        {
            var directories = Directory.EnumerateDirectories(tViewItem.ItemPath, "*", new EnumerationOptions { IgnoreInaccessible = true });
            foreach (var dir in directories)
            {
                var tvi = new TViewItem(dir);
                //tvi.MouseDoubleClick += TViewItem_MouseDoubleClick;
                //tvi.DoubleClicked += TViewItem_DoubleClicked;
                tViewItem.Items.Add(tvi);
            }
            return (true, tViewItem);
        }
        catch (UnauthorizedAccessException)
        {
            tViewItem.ToolTip = new string("locked");
            tViewItem.Foreground = Brushes.Red;
            return (false, null);
        }
        catch (Exception ex)
        {
            if (ex.Message.StartsWith("This drive is locked by BitLocker"))
            {
                Log(ex.Message);
                int startPos = ex.Message.IndexOf(":\\") -1;
                int endPas = ex.Message.Length - startPos -1;
                string driveLetter = ex.Message.Substring(startPos, endPas);
                Log(driveLetter);
                CommonMethods.OpenWith(@"C:\Windows\system32\bdeunlock.exe", driveLetter);
                //var bitlocker = new BitLocker("localhost");
                //var result = bitlocker.UnlockWithPassphrase("E:", "seagate1q2W3e4R5t");
                //tViewItem.Foreground = Brushes.Red;
                //tViewItem.ToolTip = "BitLocker";
                return (false, null);
            }
            Log(ex.Message);
            return (false, null);
        }
    }
}
