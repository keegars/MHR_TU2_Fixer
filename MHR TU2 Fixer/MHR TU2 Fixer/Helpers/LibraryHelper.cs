using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Media;
using Murmur;

namespace MHR_TU2_Fixer.Helpers
{
    public static class LibraryHelper
    {
        public static HashAlgorithm mm3h = MurmurHash.Create32(seed: 0xFFFFFFFF);

        public static string ReadUniNullTerminatedString(BinaryReader br)
        {
            var stringC = new List<char>();
            var newByte = br.ReadChar();
            while (newByte != 0)
            {
                stringC.Add(newByte);
                newByte = br.ReadChar();
            }
            return new string(stringC.ToArray());
        }

        public static Brush GetBrushFromHex(string hexColor)
        {
            return (Brush)new BrushConverter().ConvertFrom(hexColor);
        }

        public static uint Murmur3Hash(byte[] str)
        {
            return BitConverter.ToUInt32(mm3h.ComputeHash(str), 0);
        }

        public static float Clamp(float input, float min, float max)
        {
            return input > max ? max : input < min ? min : input;
        }

        public static void Swap<T>(IList<T> list, int indexA, int indexB)
        {
            var tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
        }
    }
}