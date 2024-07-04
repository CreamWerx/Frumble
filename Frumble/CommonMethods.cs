using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Frumble;
public static class CommonMethods
{
    public static void OpenWithDefaultApp(string fullPath)
    {
        try
        {
            var p = new Process
            {
                StartInfo = new ProcessStartInfo(fullPath)
                {
                    UseShellExecute = true
                }
            };
            p.Start();
        }
        catch (Exception ex)
        {
            Error(ex.Message);
        }
    }

    public static void CreateFolders(List<object> tmpList)
    {
        foreach (var item in tmpList)
        {
            CreateFolder((item as LViewItem));
        }
    }

    public static void CreateFolder(LViewItem? lViewItem)
    {
        if (lViewItem is null)
        {
            return;
        }

        string? filePath = lViewItem.ItemPath;
        (string newDir, string newPath) newDirAndPath = GetNewDirAndPath(filePath);

        Directory.CreateDirectory(newDirAndPath.newDir);

        CreateSubFolders(newDirAndPath.newDir);

        File.Move(filePath, newDirAndPath.newPath);
    }

    private static void CreateSubFolders(string newDir)
    {
        Directory.CreateDirectory(Path.Combine(newDir, "Audio"));
        Directory.CreateDirectory(Path.Combine(newDir, "Old"));
        Directory.CreateDirectory(Path.Combine(newDir, "clips"));
    }

    private static (string, string) GetNewDirAndPath(string filePath)
    {

        string? fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        string? fileName = Path.GetFileName(filePath);
        string? fileDir = Path.GetDirectoryName(filePath);
        string? newDir = Path.Combine(fileDir, fileNameWithoutExtension);
        string? newPath = Path.Combine(newDir, fileName);
        return (newDir, newPath);
    }


    public static void OpenWith(string exe, IList selectedItems)
    {
        if (selectedItems.Count == 1 && selectedItems[0] is null)
        {
            return;
        }
        string targetPaths = string.Empty;

        foreach (var item in selectedItems)
        {
            targetPaths += $"\"{((LViewItem)item).ItemPath}\" ";
        }
        targetPaths = targetPaths.TrimEnd();
        try
        {
            var p = new Process
            {
                StartInfo = new ProcessStartInfo(exe)
                {
                    Arguments = targetPaths
                }
            };
            p.Start();
        }
        catch (Exception ex)
        {
            Error(ex.Message);
        }
    }

    private static void Error(string message)
    {
        MessageBox.Show(message);
    }
}
