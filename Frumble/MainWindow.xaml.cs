global using Path = System.IO.Path;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Management;


namespace Frumble;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    // dblclk used in ListView(lv) to distinguish from MouseUp
    bool dblclk = false;
    // SelectedLVItems used for copy / paste
    IList? SelectedLVItems = null;
    // Stored value of tbCurrentPath
    string oldPath = "start";
    List<CrumbItem> crumbList= new List<CrumbItem>();

    TViewItem? TVFrequent;
    #region History Control
    /// <summary>
    /// Everything here is used in navigating the history of visited folders
    /// </summary>
    List<TViewItem> History = new();
    Logger logger = new Logger();
       
    int currentHistoryPos = 0;

    public bool HistoryNavigation { get; private set; } = false;
    public bool DoubleClickWasItem { get; private set; }
    public bool Seeking { get; private set; }

    private void btnBack_MouseUp(object sender, MouseButtonEventArgs e)
    {
        HistoryNavigation = true;
        ((TViewItem)tv.Items[0]).IsExpanded = false;
        var tvi = HistoryBack();
        tbCurrentPath.Text = tvi.ItemPath;
        //TreeViewSeekToItem(tvi.ItemPath);
        

    }

    private void btnForward_MouseUp(object sender, MouseButtonEventArgs e)
    {
        HistoryNavigation = true;
        ((TViewItem)tv.Items[0]).IsExpanded = false;
        var tvi = HistoryForward();
        tbCurrentPath.Text = tvi.ItemPath;
        //TreeViewSeekToItem(tvi.ItemPath);
    }
    #endregion

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
        //titleBar.UseAeroCaptionButtons = true;

        Log("App Start");
        LoadFrequent(tv);
        Log("Loaded frequent folders");
        ListDrives(tv);
        Log("Drives listed");
        BuildLVContextMenu();
        Log("File list view context menu built");
        btnBack.Content = "<";
        btnForward.Content = ">";
    }

    private void LoadFrequent(TreeView tv)
    {
        try
        {
            TVFrequent = new TViewItem(true, "Frequent");
            tv.Items.Add(TVFrequent);
            var paths = File.ReadAllLines(tbFrequentPath.Text);
            foreach (var item in paths)
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    TViewItem tViewItem = new TViewItem(item);
                    TVFrequent.Items.Add(tViewItem);
                }
            }
        }
        catch (Exception ex)
        {
            Error(ex.Message);
        }
    }

    private void LVOpenWithMenuItem_Clicked(object? sender, string e)
    {
        string selectedItemPath = ((LViewItem)lv.SelectedItem).ItemPath;
        string tmp = $"open \"{selectedItemPath}\" with \"{e}\"";
        CommonMethods.OpenWith(e, lv.SelectedItems);
        //Log(tmp);
    }

    private void LVSendToMenuItem_Clicked(object? sender, string e)
    {
        lvCM.IsOpen = false;
        var selectedItem = ((LViewItem)lv.SelectedItem);
        lv.UnselectAll();
        string selectedItemPath = selectedItem.ItemPath;
        string fileName = Path.GetFileName(selectedItemPath);
        string newFilePath = Path.Combine(e, fileName);
        string tmp = $"send \"{selectedItemPath}\" to \"{newFilePath}\"";
        Task.Run(()=> SendTo(selectedItem, newFilePath, tmp));
    }

   private void TViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        if (e.ChangedButton == MouseButton.Left)
        {
            TViewItem lViewItem = (TViewItem)sender;
            if (lViewItem.IsSelected)
            {
                CommonMethods.OpenWithDefaultApp(lViewItem.ItemPath);
            }
        }
    }

    private void TViewItem_DoubleClicked(object? sender, TViewItem e)
    {

        
        if (e.IsSelected)
        {
            //Log("TViewItem_DoubleClicked");
            CommonMethods.OpenWithDefaultApp(e.ItemPath);
        }
    }

    private void LViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            e.Handled = true;
            LViewItem lViewItem = (LViewItem)sender;
            CommonMethods.OpenWithDefaultApp(lViewItem.ItemPath);
            //Log($"Open file: {lViewItem.ItemPath}");
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
            (bool succss, int count) searchResults = SearchCurrentDirectory(tbSearch.Text);
        }
    }

    private TViewItem? SearchTreeView(string text)
    {
        
        var pathSplit = text.Split(Path.DirectorySeparatorChar);
        if ((pathSplit is not null) && pathSplit.Length > 0)
        {
            var Items = tv.Items;
            //Log($"pathSplit: {pathSplit.Length} items: Item 0: {pathSplit[0]}");
            foreach (var pathPart in pathSplit)
            {
                //Log($"Searching for {pathPart}");
                // Get a reference to each dir in path
                TViewItem? tViewItem = GetTViewItemByItemName(Items, pathPart);
                if (tViewItem is not null)
                {
                    //Log($"Found {tViewItem.ItemPath}");
                    //UpdateTViewItem(tViewItem);
                    tViewItem.IsSelected = true;
                    //tViewItem.IsExpanded = true;
                    Items = tViewItem.Items;
                }
            }
            
        }
        return null;
    }

    private TViewItem? GetTViewItemByItemName(ItemCollection items, string pathPart)
    {
        foreach (var item in items)
        {
            var tmpItem = (TViewItem)item;
            //Log(tmpItem.ItemName);
            if (tmpItem.ItemName == pathPart)
            {
                //Log($"returning: {tmpItem.ItemPath}");
                return tmpItem;
            }
        }
        return null;
    }

    private void Label_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        Label labelButton = (Label)sender;
        labelButton.Foreground = Brushes.LightGray;
        WrapPanel wrapPanel = (WrapPanel)labelButton.Parent;//
        GroupBox groupBox = (GroupBox)wrapPanel.Parent;//.GetType().ToString();
        string gbTitle = groupBox.Header.ToString();
        bool success = PerformAction(gbTitle, labelButton.Content.ToString());
        if (success)
        {
            Log("success leave it at that");
            return;
        }
        Log("failed leave it at that");
    }

    private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        ((Label)sender).Foreground = Brushes.Blue;
    }

    private void Label_MouseEnter(object sender, MouseEventArgs e)
    {
        var label = (Label)sender;
        label.FontWeight = FontWeights.Bold;
        label.Foreground = Brushes.White;
    }

    private void Label_MouseLeave(object sender, MouseEventArgs e)
    {
        var label = (Label)sender;
        label.FontWeight = FontWeights.Normal;
        label.Foreground = Brushes.LightGray;
    }

    private void tv_Loaded(object sender, RoutedEventArgs e)
    {
        //TreeView tree = (TreeView)sender;
        //Thumb thumb = tree.Chi<Thumb>(scrollBar);
        //Rectangle rectangle = FindVisualChild<Rectangle>(thumb);
        //rectangle.Fill = Brushes.Red;
    }

    private void lv_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (dblclk)
        {
            dblclk = false;
            return;
        }
        if (e.Source is ListView)
        {
            lv.UnselectAll();
        }
        SetFooter(lv.SelectedItems.Count);
    }

    private void lv_KeyUp(object sender, KeyEventArgs e)
    {
        if (Keyboard.IsKeyDown(Key.LeftCtrl))
        {
            if (e.Key == Key.A)
            {
                SetFooter(lv.Items.Count);
            }
            else if (e.Key == Key.C)
            {
                var selectedItems = lv.SelectedItems;
                if (selectedItems.Count > 0)
                {
                    foPasteOp.Background = Brushes.DarkOrange; 
                }
            }
        }
    }

    private void lv_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DoubleClickWasItem)
        {
            DoubleClickWasItem = false;
            return;
        }
        //Log("lv fired");
        if (e.Source is ListView)
        {
            //foreach (var item in lv.Items)
            //{
            //    ((LViewItem)item).IsSelected = true;
            //}
            lv.SelectAll();
        }
        SetFooter(lv.SelectedItems.Count);
        dblclk = true;
    }

    private void btnClearLog_Click(object sender, RoutedEventArgs e)
    {
        tblLog.Text = "";
    }

    private void MenuItem_Click(object sender, RoutedEventArgs e)
    {

    }

    private LViewItem GetSelectedLVItem(object sender)
    {
        return (LViewItem)sender;
    }

    private void WindowChrome_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(e.OriginalSource.ToString());
    }

    private void tbCurrentPath_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            SearchTreeView(tbCurrentPath.Text); 
        }
    }

    private void tbCurrentPath_LostFocus(object sender, RoutedEventArgs e)
    {//part of determining if clicked out of tbCurrentPath

        ToggleBreadCrumb(Visibility.Visible);
    }

    private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
    {//part of determining if clicked out of tbCurrentPath

        //take focus away from focused control
        mainGrid.Focus();
    }

    private void tv_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        e.Handled = true;
        if(Seeking)
        {
            Seeking = false;
            return;
        }
        TViewItem tViewItem = (TViewItem)((TreeView)sender).SelectedItem;
        //CollapseTreeviewItems(tViewItem);
        if ((string)tViewItem.Header == "Frequent")
        {
            //DealwithFrequentClick(tViewItem);
            return;
        }
        var upTV = UpdateTViewItem(tViewItem);
        if (upTV.success)
        {
            if (!HistoryNavigation)
            {
                History.Add(tViewItem);
                currentHistoryPos = History.Count - 1;
                btnForward.IsEnabled = false;
            }
            tbCurrentPath.Text = tViewItem.ItemPath;
            btnBack.IsEnabled = true;
            tViewItem.IsExpanded = true;
            //TreeViewSeekToItem(tbCurrentPath.Text);
            ScrollTviewItemsIntoView(tViewItem);
            //ToggleBreadCrumb(Visibility.Hidden);
            var popLV = PopulateListView(tViewItem.ItemPath, lv);
        }
        HistoryNavigation = false;
    }

    private void tbCurrentPath_TextChanged(object sender, TextChangedEventArgs e)
    {
        if(tbCurrentPath.Text != oldPath)
        {
            Debug.WriteLine(oldPath);
            Debug.WriteLine(tbCurrentPath.Text);
            Debug.WriteLine("##########");
            oldPath = tbCurrentPath.Text;
            TreeViewSeekToItem(tbCurrentPath.Text);
            ToggleBreadCrumb(Visibility.Hidden);
        }
        // TODO Update crumbs
        //UpdateCrumbs(tbCurrentPath.Text);
    }

    private void crumbSV_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        //return;
        Log("crumbSV_MouseLeftButtonUp");
        ToggleBreadCrumb(Visibility.Visible);
    }
}




