using System;
using System.IO;
using static MHR_TU2_Fixer.Helpers.FolderHelper;
using static MHR_TU2_Fixer.Helpers.MDFHelper;
using static MHR_TU2_Fixer.MDF.MDFEnums;

namespace MHR_TU2_Fixer
{
    public static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            var baseFolder = PickStaticFolder(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));

            if (!baseFolder.Exists)
            {
                Environment.Exit(0);
            }

            //Get conversion folders
            var conversionBaseFolder = CreateFolder(Environment.CurrentDirectory, "Conversions");
            var conversionFolder = conversionBaseFolder.CreateSubdirectory($"{DateTime.Now:yyyyMMdd_HHmmss}_{Path.GetFileName(baseFolder.FullName)}");

            //Generate the prefabs, and update them to TU1 and TU2
            //Copy over all files in same format to folder, and attempt conversion on the folder
            CloneDirectory(baseFolder, conversionFolder
                ,
                "*.pfb.17"
                );

            PrefabFixer.GeneratePrefabs(baseFolder, conversionFolder);

            //Convert the MDF Files
            //Copy over all files in same format to folder, and attempt conversion on the folder

            CloneDirectory(baseFolder, conversionFolder
                ,
                "*.mdf2.23"
                );

            ConvertMDFFiles(GetFiles(conversionFolder.FullName, "*.mdf2.23"), MDFConversion.MergeAndAddMissingProperties);

            //Open Folder Location with file explorer
            OpenExplorerLocation(conversionFolder.FullName);
        }
    }
}