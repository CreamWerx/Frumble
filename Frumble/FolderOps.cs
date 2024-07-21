using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualBasic.FileIO;

namespace Frumble;
public partial class MainWindow
{
    private void CopyPasteDir()
    {
        //return;
        try
        {
            var item = (CBItem)cmboCopyPasteDir.Items[0];
            // TODO This only copies contents, need to create target folder
            string newDir = Path.Combine(tbCurrentPath.Text, Path.GetFileName(item.ItemPath));
            if (!Directory.Exists(newDir))
            {
                Directory.CreateDirectory(newDir);
                Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(item.ItemPath, newDir, false);
            }
            UpdateTreeViewItem((TViewItem)tv.SelectedItem);
            cmboCopyPasteDir.Items.Clear();

        }
        catch (Exception ex)
        {
            Log(ex.Message, true);
        }
    }

    private void CopyDir()
    {
        var selectedTVItem = (TViewItem)tv.SelectedItem;
        CBItem cbi = new CBItem(selectedTVItem);
        cmboCopyPasteDir.Items.Add(cbi);
    }
    private void CutPasteDir()
    {
        throw new NotImplementedException();
    }

    private void CutDir()
    {
        var selectedTVItem = (TViewItem)tv.SelectedItem;
        MessageBox.Show($"{selectedTVItem.ItemPath}");
    }
}
