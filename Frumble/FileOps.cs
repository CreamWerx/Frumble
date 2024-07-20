using System.Collections;
using System.IO;
using System.Windows.Controls;

namespace Frumble;
public partial class MainWindow
{
    List<LViewItem> CutList = new List<LViewItem>();
    List<LViewItem> CopyList = new List<LViewItem>();

    private string CutPaste()
    {
        int count = 0;
        List<CBItem> toCutItems = new List<CBItem>();
        foreach (var item in cmboCutPaste.Items)
        {
            var cbItem = (CBItem)item;
            if (cbItem.IsItemChecked)
            {
                count++;
                toCutItems.Add(cbItem);
            }
        }
        FilesCut(toCutItems, tbCurrentPath.Text);
        if (CutList.Count < 1)
        {
            cmboCutPaste.Items.Clear();
        }
        return $"Cut paste {count} items?";
    }

    private string Cut()
    {
        if (lv.SelectedItems is null)
        {
            return "null";
        }
        
        if (CutList.Count == 0)
        {
            cmboCutPaste.Items.Clear();
            var cbItem = new CBItem("Select All");
            cbItem.SelectAllChecked += CbItem_SelectAllChecked;
            cbItem.SelectAllUnChecked += CbItem_SelectAllUnChecked;
            cmboCutPaste.Items.Add(cbItem);
        }
        CutList = FilesAddToCutList(lv.SelectedItems);
        //cmboCopyPaste.Items.Clear();
        foreach (var ListItem in CutList)
        {
            bool alreadyExists = false;
            var cbItem = new CBItem(ListItem);
            foreach (var CollectionItem in cmboCutPaste.Items)
            {
                //alreadyExists = false;
                var item = (CBItem)CollectionItem;
                if (cbItem.ItemPath == item.ItemPath)
                {
                    alreadyExists = true;
                    break;
                }
            }
            if (!alreadyExists)
            {
                cmboCutPaste.Items.Add(cbItem);
            }
        }
        //var count = cmboCutPaste.Items.Count;
        return lv.SelectedItems.Count.ToString();
    }

    private string CopyPaste()
    {
        int count = 0;
        List<CBItem> toCopyItems = new List<CBItem>();
        foreach (var item in cmboCopyPaste.Items)
        {
            var cbItem = (CBItem)item;
            if (cbItem.IsItemChecked)
            {
                count++;
                toCopyItems.Add(cbItem);
            }
        }
        FilesCopy(toCopyItems, tbCurrentPath.Text);
        if (CopyList.Count < 1)
        {
            cmboCopyPaste.Items.Clear();
        }
        return $"Cut paste {count} items?";



        //int count = 0;
        //foreach (var item in cmboCopyPaste.Items)
        //{
        //    var cbItem = (CBItem)item;
        //    if (cbItem.IsItemChecked)
        //    {
        //        count++;
        //    }
        //}
        //return $"Copy paste {count} items?";
    }

    private string Copy()
    {
        if (lv.SelectedItems is null)
        {
            return "null";
        }

        if (CopyList.Count == 0)
        {
            cmboCopyPaste.Items.Clear();
            var cbItem = new CBItem("Select All");
            cbItem.SelectAllChecked += CbItem_SelectAllChecked;
            cbItem.SelectAllUnChecked += CbItem_SelectAllUnChecked;
            cmboCopyPaste.Items.Add(cbItem);
        }
        CopyList = FilesAddToCopyList(lv.SelectedItems);
        //cmboCopyPaste.Items.Clear();
        foreach (var ListItem in CopyList)
        {
            bool alreadyExists = false;
            var cbItem = new CBItem(ListItem);
            foreach (var CollectionItem in cmboCopyPaste.Items)
            {
                //alreadyExists = false;
                var item = (CBItem)CollectionItem;
                if (cbItem.ItemPath == item.ItemPath)
                {
                    alreadyExists = true;
                    break;
                }
            }
            if (!alreadyExists)
            {
                cmboCopyPaste.Items.Add(cbItem);
            }
        }
        //var count = cmboCutPaste.Items.Count;
        return lv.SelectedItems.Count.ToString();
    }

    public List<LViewItem> FilesAddToCutList(IList items)
    {
        Log("Cut:");
        foreach (var item in items)
        {
            var lvi = (LViewItem)item;
            bool containsItem = CutList.Any(i => i.ItemPath == lvi.ItemPath);
            if (!containsItem)
            {
                Log(lvi.ItemPath);
                CutList.Add(lvi); 
            }
        }
        return CutList;//.Distinct().ToList();
    }

    public List<LViewItem> FilesAddToCopyList(IList items)
    {
        foreach (var item in items)
        {
            var lvi = (LViewItem)item;
            bool containsItem = CopyList.Any(i => i.ItemPath == lvi.ItemPath);
            if (!containsItem)
            {
                Log(lvi.ItemPath);
                CopyList.Add(lvi);
            }
        }
        return CopyList;//.Distinct().ToList();
    }

    public void FilesCut(IList items, string dirPath)
    {
        Log("Paste: ");
        foreach (var item in items)
        {
            var cbi = (CBItem)item;
            var cutToPath = Path.Combine(dirPath, cbi.ItemName);
            Log($"{cbi.ItemPath} to {cutToPath}");
            try
            {
                File.Move(cbi.ItemPath, cutToPath);
                cbi.FileOpSuccess = true;
                
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("exists"))
                {
                    cbi.FileOpSuccess = null;
                }
                else
                {
                    cbi.FileOpSuccess = false;
                }
                Log(ex.Message, true);
            }
            Log($"cbi.FileOpSuccess - {cbi.FileOpSuccess.ToString()}");

        }
        PopulateListView(dirPath, lv);
        foreach (var newitem in items)
        {
            var cbi = (CBItem)newitem;
            foreach (var item in lv.Items)
            {
                var lvi = (LViewItem)item;
                if (lvi.ItemName == cbi.ItemName)
                {
                    ControlSuccess(lvi, cbi.FileOpSuccess);
                    cbi.FileOpSuccess = null;
                    break;
                }
            }
        }
        foreach (var item in items)
        {
            var cbi = (CBItem)item;
            CutList.Remove(cbi.ItemLVItem);
            cmboCutPaste.Items.Remove(cbi);
        }
    }

    public void FilesCopy(IList items, string dirPath, bool overWrite = false)
    {
        foreach (var item in items)
        {
            var cbi = (CBItem)item;
            var copyToPath = Path.Combine(dirPath, cbi.ItemName);
            Log($"{cbi.ItemPath} to {copyToPath}");
            try
            {
                File.Copy(cbi.ItemPath, copyToPath, overWrite);
                cbi.FileOpSuccess = true;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("exists"))
                {
                    cbi.FileOpSuccess = null;
                }
                else
                {
                    cbi.FileOpSuccess = false;
                }
                Log(ex.Message, true);
            }
            Log($"cbi.FileOpSuccess - {cbi.FileOpSuccess.ToString()}");
        }
        
        PopulateListView(dirPath, lv);
        foreach (var newitem in items)
        {
            var cbi = (CBItem)newitem;
            foreach (var item in lv.Items)
            {
                var lvi = (LViewItem)item;
                if (lvi.ItemName == cbi.ItemName)
                {
                    ControlSuccess(lvi, cbi.FileOpSuccess);
                    break;
                }
            }
            
        }

        foreach (var item in items)
        {
            var cbi = (CBItem)item;
            CopyList.Remove(cbi.ItemLVItem);
            cmboCopyPaste.Items.Remove(cbi);
        }



        //Log("Paste: ");
        //foreach (var item in items)
        //{
        //    var cbi = (CBItem)item;
        //    var copyToPath = Path.Combine(dirPath, cbi.ItemName);
        //    Log($"{cbi.ItemPath} to {copyToPath}");
        //    File.Copy(cbi.ItemPath, copyToPath);
        //    //ControlSuccess(cbi, true);
        //}
        //CopyList.Clear();
    }
}
