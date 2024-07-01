using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        ListDrives(tv);
        BuildLVContextMenu();
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

    private void Label_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        ((Label)sender).Foreground = Brushes.LightGray;
    }

    private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        ((Label)sender).Foreground = Brushes.Blue;
    }

    private void Label_MouseEnter(object sender, MouseEventArgs e)
    {
        ((Label)sender).Foreground = Brushes.Ivory;
    }

    private void Label_MouseLeave(object sender, MouseEventArgs e)
    {
        ((Label)sender).Foreground = Brushes.LightGray;
    }

    private void tv_Loaded(object sender, RoutedEventArgs e)
    {
        //TreeView tree = (TreeView)sender;
        //Thumb thumb = tree.Chi<Thumb>(scrollBar);
        //Rectangle rectangle = FindVisualChild<Rectangle>(thumb);
        //rectangle.Fill = Brushes.Red;
    }
}