using System.Collections;
using System.IO;

namespace Frumble;
public partial class MainWindow
{
    List<LViewItem> CutList = new List<LViewItem>();
    List<LViewItem> CopyList = new List<LViewItem>();

    public void FilesAddToCutList(IList items)
    {
        Log("Copy:");
        foreach (var item in items)
        {
            var lvi = (LViewItem)item;
            Log(lvi.ItemPath);
            CutList.Add(lvi);
        }
    }

    public void FilesAddToCopyList(IList items)
    {
        Log("Copy:");
        foreach (var item in items)
        {
            var lvi = (LViewItem)item;
            Log(lvi.ItemPath);
            CopyList.Add(lvi);
            //var cmbi = new CBItem(lvi);
            //cmboCutPaste.Items.Add(cmbi);
        }
    }

    public void FilesCut(IList items, string dirPath)
    {
        Log("Paste: ");
        foreach (var item in items)
        {
            var lvi = (LViewItem)item;
            var copyToPath = Path.Combine(dirPath, Path.GetFileName(lvi.ItemPath));
            Log($"{lvi.ItemPath} to {copyToPath}");
            //File.Move(lvi.ItemPath, copyToPath);
            ControlSuccess(lvi, true);
        }
        CutList.Clear();
    }


    public void FilesCopy(IList items, string dirPath)
    {
        Log("Paste: ");
        foreach (var item in items)
        {
            var lvi = (LViewItem)item;
            var copyToPath = Path.Combine(dirPath, Path.GetFileName(lvi.ItemPath));
            Log($"{lvi.ItemPath} to {copyToPath}");
            //File.Copy(lvi.ItemPath, copyToPath);
            ControlSuccess(lvi, true);
        }
        CopyList.Clear();
    }
}
