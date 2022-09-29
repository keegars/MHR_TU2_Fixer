using System.Collections.Generic;
using System.IO;
using System.Text;
using static MHR_TU2_Fixer.Helpers.LibraryHelper;

namespace MHR_TU2_Fixer.MDF
{
    public class TextureBinding
    {
        public int NameOffsetIndex;
        public int PathOffsetIndex;
        public string name { get; set; }
        public string path { get; set; }

        public TextureBinding()
        {
            name = "BaseMetalMap";
            path = "systems/rendering/nullblack.tex";
        }

        public TextureBinding(string pName, string pPath)
        {
            name = pName;
            path = pPath;
        }

        public int GetSize()
        {
            return 32;
        }

        public void Export(BinaryWriter bw, ref long offset, long stringTableOffset, List<int> strTableOffs)
        {
            bw.BaseStream.Seek(offset, SeekOrigin.Begin);
            bw.Write(stringTableOffset + strTableOffs[NameOffsetIndex]);
            bw.Write(Murmur3Hash(Encoding.Unicode.GetBytes(name)));
            bw.Write(Murmur3Hash(Encoding.ASCII.GetBytes(name)));
            bw.Write(stringTableOffset + strTableOffs[PathOffsetIndex]);
            bw.Write((long)0);

            offset += GetSize();
        }
    }
}