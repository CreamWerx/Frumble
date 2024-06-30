using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Frumble;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    IList? SelectedLVItems = null; 
    public MainWindow()
    {
        InitializeComponent();
    }

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
        catch(Exception ex)
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

    private void LViewItem_MouseEnter(object sender, MouseEventArgs e)
    {
        LViewItem lViewItem = (LViewItem)sender;
        if (lViewItem.Bytes == -1)
        {
            string suffix = "b";
            int bytes = (int)new FileInfo(lViewItem.ItemPath).Length;
            if (bytes >= 1024)
            {
                suffix = "Kb";
                bytes = bytes / 1024;
                if (bytes >= 1024)
                {
                    bytes = bytes / 1024;
                    suffix = "Mb";
                }
                
            }
            
            lViewItem.Bytes = bytes;
            string tt = (lViewItem.ToolTip as string);
            tt += $"{Environment.NewLine}Length: {lViewItem.Bytes}{suffix}";
            lViewItem.ToolTip = tt;
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

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        ListDrives(tv);
        BuildLVContextMenu();
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

    private MenuItemEx CreateOpenWithMenuItem(string exePath)
    {
        var menuItem = new MenuItemEx(exePath);
        menuItem.Clicked += LVMenuItem_Clicked;

        return menuItem;
    }

    private void LVMenuItem_Clicked(object? sender, string e)
    {
        string selectedItem = ((LViewItem)lv.SelectedItem).ItemPath;
        string tmp = $"open \"{selectedItem}\" with \"{e}\"";
        //tmpOutput.Text = tmp;
    }

    //private void MenuItem_Click(object sender, RoutedEventArgs e)
    //{
    //    if (SelectedLVItems != null)
    //    {
    //        foreach (var item in SelectedLVItems)
    //        {
    //            var lViewItem = (LViewItem)item;
    //            Debug.WriteLine($"{((MenuItem)e.Source).Header} {lViewItem.ItemPath}");
    //        } 
    //    }
    //}

    private void TViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            TViewItem lViewItem = (TViewItem)sender;
            if (lViewItem.IsSelected)
            {
                CommonMethods.OpenWithDefaultApp(lViewItem.ItemPath);
            }
        }
    }

    private void LViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        LViewItem lViewItem = (LViewItem)sender;
        CommonMethods.OpenWithDefaultApp(lViewItem.ItemPath);
    }

    private void tv_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        e.Handled = true;
        TViewItem tViewItem = (TViewItem)((TreeView)sender).SelectedItem;
        var upTV = UpdateTViewItem(tViewItem);
        if (upTV.success)
        {
            tbCurrentPath.Text = tViewItem.ItemPath;
            var popLV = PopulateListView(tViewItem.ItemPath, lv);
        }
    }

    private void lvCM_Opened(object sender, RoutedEventArgs e)
    {
        SelectedLVItems = lv.SelectedItems;
        DisableRedundantMenuItems();
    }

    private void DisableRedundantMenuItems()
    {
        if (SelectedLVItems?.Count < 1) { openWith.IsEnabled = false; return; } else openWith.IsEnabled = true;
        if (!((LViewItem)lv.SelectedItem).ItemPath.EndsWith(".exe")) addThisApp.IsEnabled = false; else addThisApp.IsEnabled = true;
    }

    private void lvCM_Closed(object sender, RoutedEventArgs e)
    {
        SelectedLVItems = null;
    }

    private void AddThisApp_Click(object sender, RoutedEventArgs e)
    {
        var exePath = ((LViewItem)lv.SelectedItem).ItemPath;
        AddAppToOpenWithMenu(exePath);
    }

    private void AddAppToOpenWithMenu(string exePath)
    {
        MenuItemEx menuItemEx = CreateOpenWithMenuItem(exePath);
        openWith.Items.Add(menuItemEx);
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        string paths = "";
        foreach (var item in openWith.Items)
        {
            try
            {
                var miex = (MenuItemEx)item;
                paths += $"{miex.ExePath}{Environment.NewLine}";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            File.WriteAllText(openWithPath.Text, paths.TrimEnd());
        }
    }

    private void tbSearch_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            (bool success, int count) searchResults = SearchCurrentDirectory(tbSearch.Text);
        }
    }

    private (bool success, int count) SearchCurrentDirectory(string text)
    {
        MessageBox.Show("NotImplementedException");
        return (false, 0);
    }
}