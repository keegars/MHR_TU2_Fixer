using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace MHR_TU2_Fixer
{
    public static class PrefabFixer
    {
        [STAThreadAttribute]
        public static void GeneratePrefabs(DirectoryInfo baseFolder, DirectoryInfo conversionFolder)
        {
            var prefabs = Directory.GetFiles(conversionFolder.FullName, "*.pfb.17", SearchOption.AllDirectories);

            //TU1 Conversion
            var oldPrefabHex = "66 e1 a6 8f 06 6d d5 ed d1 07 28 e8 bb dd 1d 11";
            var oldPrefabBytes = HexStringToByte(oldPrefabHex);

            var newPrefabHex = "66 e1 a6 8f 46 5f 73 52 d1 07 28 e8 bb dd 1d 11";
            var newPrefabBytes = HexStringToByte(newPrefabHex);

            ConvertPrefabs(prefabs, oldPrefabBytes, newPrefabBytes);

            //TU2 Conversion
            oldPrefabHex = "46 5F 73 52 D1 07 28 E8 BB DD 1D 11";
            oldPrefabBytes = HexStringToByte(oldPrefabHex);

            newPrefabHex = "7F D7 47 7F D1 07 28 E8 68 20 A6 CB";
            newPrefabBytes = HexStringToByte(newPrefabHex);

            ConvertPrefabs(prefabs, oldPrefabBytes, newPrefabBytes);
        }

        private static  string ReplaceHexFromString(string hex, string source, string replacement)
        {
            var hexSource = AddBlankHex(AsciiToHexString(source));
            var hexReplacement = AddBlankHex(AsciiToHexString(replacement));
            
            return hex.Replace(hexSource, hexReplacement);
        }

        private static void ConvertPrefabs(string[] prefabs, byte[] oldPrefabBytes, byte[] newPrefabBytes)
        {
            foreach (var prefab in prefabs)
            {
                if (prefab.Contains("helm"))
                {
                    //Need to recreate the helm file, so let's use a premade one and replace the file names inside with the armor id!
                    var filename = Path.GetFileName(prefab);
                    var fileBytes = File.ReadAllBytes(prefab);
                    var prefabBytes = File.ReadAllBytes($@"{Environment.CurrentDirectory}\Prefab\example\TU2\f_helm001.pfb.17");

                    var armorId = int.Parse(new string(filename.Replace(".pfb.17", "").Where(z => char.IsDigit(z)).ToArray())).ToString("000");
                    var isMale = filename.Contains("m_helm");

                    var fileHex = ByteToHexString(fileBytes);
                    var prefabHex = ByteToHexString(prefabBytes);

                    var newHex = prefabHex + string.Empty;

                    newHex = ReplaceHexFromString(newHex, "f_helm001", (isMale ? "m" : "f") + "_helm" + armorId);
                    newHex = ReplaceHexFromString(newHex, "pl001", "pl" + armorId);

                    if (isMale)
                    {
                        newHex = ReplaceHexFromString(newHex, "mod/f/", "mod/m/");
                    }
                    
                    var bytes = HexStringToByte(newHex);
                    File.WriteAllBytes(prefab, bytes);
                }
                else
                {
                    var prefabBytes = File.ReadAllBytes(prefab);

                    if (ContainsBytes(prefabBytes, oldPrefabBytes))
                    {
                        //Attempt conversion, and copy over
                        Console.WriteLine($"{prefab} contains old prefab bytes, will attempt to convert");

                        var newPrefab = ReplaceBytes(prefabBytes, oldPrefabBytes, newPrefabBytes);

                        File.WriteAllBytes(prefab, newPrefab);

                        //Add check to make sure that the bytes got written properly and are the correct length
                        var tmpNewFileBytes = File.ReadAllBytes(prefab);

                        if (newPrefab.Length != tmpNewFileBytes.Length || !BytesSimilar(newPrefab, tmpNewFileBytes))
                        {
                            throw new Exception($"{prefab} has encountered an issue where the written bytes do not match the actual bytes, this may be caused due to the folder being on a different drive or folder permissions. Please try moving the prefab fixer to the same drive as the folder, or run this program as administrator.");
                        }
                    }
                    else if (ContainsBytes(prefabBytes, newPrefabBytes))
                    {
                        //Don't convert, just output message
                        Console.WriteLine($"{prefab} contains new prefab bytes, no need to convert.");
                    }
                    else
                    {
                        //Throw exception and warn of issue
                        //throw new Exception($"{prefab} does not contain any sequence for new and old prefab bytes, this has been thrown to avoid converting it.");
                    }
                }

              
            }
        }

        private static bool BytesSimilar(byte[] bytes1, byte[] bytes2)
        {
            if (bytes1.Length != bytes2.Length)
            {
                return false;
            }

            for (var i = 0; i < bytes1.Length; i++)
            {
                if (bytes1[i] != bytes2[i])
                {
                    return false;
                }
            }

            return true;
        }

        private static string AddBlankHex(string hexString)
        {
            var list = Enumerable
                .Range(0, hexString.Length / 2)
                .Select(i => hexString.Substring(i * 2, 2));

            return string.Join("00", list);
        }

        private static string AsciiToHexString(string str)
        {
            var sb = new StringBuilder();

            var bytes = Encoding.ASCII.GetBytes(str);
            foreach (var t in bytes)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString();
        }

        private static byte[] ReplaceBytes(byte[] bytes, byte[] search, byte[] replace)
        {
            var byteString = ByteToHexString(bytes);
            var searchString = ByteToHexString(search);
            var replaceString = ByteToHexString(replace);

            var newString = byteString.Replace(searchString, replaceString);

            return HexStringToByte(newString);
        }

        private static bool ContainsBytes(byte[] haystack, byte[] needle)
        {
            return SearchBytes(haystack, needle) >= 0;
        }

        private static int SearchBytes(byte[] haystack, byte[] needle)
        {
            var len = needle.Length;
            var limit = haystack.Length - len;
            for (var i = 0; i <= limit; i++)
            {
                var k = 0;
                for (; k < len; k++)
                {
                    if (needle[k] != haystack[i + k]) break;
                }
                if (k == len) return i;
            }
            return -1;
        }

        private static string ByteToHexString(byte[] bytes)
        {
            var hexString = new StringBuilder();

            foreach (int bytePart in bytes)
            {
                hexString.Append(string.Format("{0:X2}", bytePart));
            }

            return hexString.ToString();
        }

        private static byte[] HexStringToByte(string hexString)
        {
            if (hexString.Contains(" "))
            {
                hexString = hexString.Replace(" ", "");
            }

            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
            }

            var data = new byte[hexString.Length / 2];
            for (var index = 0; index < data.Length; index++)
            {
                var byteValue = hexString.Substring(index * 2, 2);
                data[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return data;
        }
    }
}