using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static MHR_TU2_Fixer.MDF.MDFEnums;

namespace MHR_TU2_Fixer.MDF
{
    public class MDFFile
    {
        public static readonly byte[] Magic = { (byte)'M', (byte)'D', (byte)'F', 0x00 };
        public static List<ShadingType> ShadingTypes = Enum.GetValues(typeof(ShadingType)).Cast<ShadingType>().ToList();
        public string FileName = "";
        private string _Header;
        ushort unkn = 1;
        public string Header { get { return _Header; } set { _Header = value; } }
        public List<Material> Materials { get; set; }

        public MDFFile(string fileName, BinaryReader br)
        {
            Materials = new List<Material>();
            Header = fileName;
            FileName = fileName;
            var mBytes = br.ReadBytes(4);
            if (Encoding.ASCII.GetString(mBytes) != Encoding.ASCII.GetString(Magic))
            {
                Console.WriteLine("Not a valid MDF file!");
                return;
            }
            var unkn1 = br.ReadUInt16();
            if (unkn1 != unkn)
            {
                Console.WriteLine("Potentially bad MDF file.");
            }
            var MaterialCount = br.ReadUInt16();
            br.ReadUInt64();
            for (var i = 0; i < MaterialCount; i++)
            {
                Materials.Add(new Material(br, i));
            }
        }

        public List<byte> GenerateStringTable(ref List<int> offsets)
        {
            var strings = new List<string>();
            for (var i = 0; i < Materials.Count; i++)
            {
                if (!strings.Contains(Materials[i].Name))
                {
                    strings.Add(Materials[i].Name);
                    Materials[i].NameOffsetIndex = strings.Count - 1;
                }
                else
                {
                    Materials[i].NameOffsetIndex = strings.FindIndex(name => name == Materials[i].Name);
                }
                if (!strings.Contains(Materials[i].MasterMaterial))
                {
                    strings.Add(Materials[i].MasterMaterial);
                    Materials[i].MMOffsetIndex = strings.Count - 1;
                }
                else
                {
                    Materials[i].MMOffsetIndex = strings.FindIndex(name => name == Materials[i].MasterMaterial);
                }
            }
            for (var i = 0; i < Materials.Count; i++)
            {
                for (var j = 0; j < Materials[i].Textures.Count; j++)
                {
                    if (!strings.Contains(Materials[i].Textures[j].name))
                    {
                        strings.Add(Materials[i].Textures[j].name);
                        Materials[i].Textures[j].NameOffsetIndex = strings.Count - 1;
                    }
                    else
                    {
                        Materials[i].Textures[j].NameOffsetIndex = strings.FindIndex(name => name == Materials[i].Textures[j].name);
                    }
                    if (!strings.Contains(Materials[i].Textures[j].path))
                    {
                        strings.Add(Materials[i].Textures[j].path);
                        Materials[i].Textures[j].PathOffsetIndex = strings.Count - 1;
                    }
                    else
                    {
                        Materials[i].Textures[j].PathOffsetIndex = strings.FindIndex(name => name == Materials[i].Textures[j].path);
                    }
                }
            }
            for (var i = 0; i < Materials.Count; i++)
            {
                for (var j = 0; j < Materials[i].Properties.Count; j++)
                {
                    if (!strings.Contains(Materials[i].Properties[j].name))
                    {
                        strings.Add(Materials[i].Properties[j].name);
                        Materials[i].Properties[j].NameOffsetIndex = strings.Count - 1;
                    }
                    else
                    {
                        Materials[i].Properties[j].NameOffsetIndex = strings.FindIndex(name => name == Materials[i].Properties[j].name);
                    }
                }
            }
            var outputBuff = new List<byte>();
            offsets.Add(0);
            for (var i = 0; i < strings.Count; i++)
            {
                var inBytes = Encoding.Unicode.GetBytes(strings[i]);
                for (var j = 0; j < inBytes.Length; j++)
                {
                    outputBuff.Add(inBytes[j]);
                }
                outputBuff.Add(0);
                outputBuff.Add(0);
                offsets.Add(outputBuff.Count);//think this will end with the very last one being unused but that's fine
            }
            return outputBuff;
        }
    }
}