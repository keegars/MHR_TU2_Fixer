using System;
using System.Diagnostics;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace MHR_TU2_Fixer.Helpers
{
    public static class FolderHelper
    {
        public static string[] GetFiles(string path, string search)
        {
            return Directory.GetFiles(path, search, SearchOption.AllDirectories);
        }

        public static void OpenExplorerLocation(string path)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                Arguments = path,
                FileName = "explorer.exe"
            };

            Process.Start(startInfo);
        }

        public static DirectoryInfo CreateFolder(params string[] path)
        {
            var folder = new DirectoryInfo(Path.Combine(path));

            if (!folder.Exists)
            {
                folder.Create();
            }

            return folder;
        }

        public static DirectoryInfo PickStaticFolder(params string[] path)
        {
            var folderPath = string.Empty;

            var folderDialog = new CommonOpenFileDialog();
            folderDialog.IsFolderPicker = true;
            folderDialog.InitialDirectory = path.Length != 0 ? Path.Combine(path) : Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            var dialogResult = folderDialog.ShowDialog();

            if (dialogResult == CommonFileDialogResult.Ok)
            {
                folderPath = folderDialog.FileName;
            }

            return new DirectoryInfo(folderPath);
        }

        public static void CloneDirectory(DirectoryInfo root, DirectoryInfo dest, string searchPattern = "*")
        {
            foreach (var directory in root.GetDirectories())
            {
                string dirName = Path.GetFileName(directory.FullName);
                if (!Directory.Exists(Path.Combine(dest.FullName, dirName)))
                {
                    try
                    {
                        Directory.CreateDirectory(Path.Combine(dest.FullName, dirName));
                    }
                    catch (Exception ex)
                    {
                        if (ex is System.IO.PathTooLongException)
                        {
                            var newDirName = @"\\?\" + dest.FullName + @"\" + dirName;
                            Directory.CreateDirectory(newDirName);
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                }
                CloneDirectory(directory, new DirectoryInfo(Path.Combine(dest.FullName, dirName)), searchPattern);
            }

            foreach (var file in root.GetFiles(searchPattern))
            {
                File.Copy(file.FullName, Path.Combine(dest.FullName, Path.GetFileName(file.FullName)), true);
            }
        }
    }
}