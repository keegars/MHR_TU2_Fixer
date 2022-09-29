using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Media;
using static MHR_TU2_Fixer.Helpers.LibraryHelper;

namespace MHR_TU2_Fixer.MDF
{
    public interface IVariableProp
    {
        int NameOffsetIndex { get; set; }
        int ValOffset { get; set; }
        string name { get; set; }
        object value { get; set; }
        int[] indexes { get; set; }
        int GetSize();

        int GetPropHeaderSize();

        void Export(BinaryWriter bw, ref long propHeadOff, ref long propOff, long basePropOff, long stringTableOff, List<int> strTableOffs);
    }
    public class Float
    {
        public float data { get; set; }
        public Float(float fData)
        {
            data = fData;
        }
    }

    public class Float4
    {
        private Color _mColor;
        private Brush _Brush;
        private float _X;
        private float _Y;
        private float _Z;
        private float _W;

        public float x { get { return _X; } set { _X = value; UpdateColor(); } }
        public float y { get { return _Y; } set { _Y = value; UpdateColor(); } }
        public float z { get { return _Z; } set { _Z = value; UpdateColor(); } }
        public float w { get { return _W; } set { _W = value; UpdateColor(); } }
        public Color mColor { get { return _mColor; } set { _mColor = value; UpdateBrush(); } }
        public Brush mBrush { get { return _Brush; } set { _Brush = value; } }
        public Float4(float fX, float fY, float fZ, float fW)
        {
            x = fX;
            y = fY;
            z = fZ;
            w = fW;
        }

        public void UpdateBrush()
        {
            byte[] hexArray = { _mColor.R, _mColor.G, _mColor.B };
            var hexBrush = "#" + BitConverter.ToString(hexArray).Replace("-", "");
            mBrush = GetBrushFromHex(hexBrush);
        }

        public void UpdateColor()
        {
            _mColor.ScR = Clamp(x, 0, 1);
            _mColor.ScG = Clamp(y, 0, 1);
            _mColor.ScB = Clamp(z, 0, 1);
            _mColor.ScA = Clamp(w, 0, 1);
            UpdateBrush();
        }
    }

    public class FloatProperty : IVariableProp
    {
        private string _Name;
        private Float _Default;
        public int NameOffsetIndex { get; set; }
        public int ValOffset { get; set; }
        public string name
        {
            get
            {
                return _Name;
            }

            set
            {
                _Name = value;
            }
        }

        public object value
        {
            get
            {
                return _Default;
            }

            set
            {
                _Default = (Float)value;
            }
        }

        public int[] indexes { get; set; }
        public FloatProperty(string Name, Float Value, int matIndex, int propIndex)
        {
            indexes = new int[2];
            name = Name;
            value = Value;
            indexes[0] = matIndex;
            indexes[1] = propIndex;
        }

        public int GetSize()
        {
            return 4;
        }

        public int GetPropHeaderSize()
        {
            return 24;
        }

        public void Export(BinaryWriter bw, ref long propHeadOff, ref long propOff, long basePropOff, long stringTableOff, List<int> strTableOffs)
        {
            var innerPropOff = (uint)(propOff - basePropOff);
            bw.BaseStream.Seek(propHeadOff, SeekOrigin.Begin);
            bw.Write(stringTableOff + strTableOffs[NameOffsetIndex]);
            bw.Write(Murmur3Hash(Encoding.Unicode.GetBytes(name)));
            bw.Write(Murmur3Hash(Encoding.ASCII.GetBytes(name)));
            bw.Write(innerPropOff);
            bw.Write(1);
            propHeadOff += GetPropHeaderSize();

            bw.BaseStream.Seek(propOff, SeekOrigin.Begin);
            bw.Write(_Default.data);
            propOff += GetSize();
        }
    }

    public class Float4Property : IVariableProp
    {
        private string _Name;
        private Float4 _Default;
        public int NameOffsetIndex { get; set; }
        public int ValOffset { get; set; }
        public string name
        {
            get
            {
                return _Name;
            }

            set
            {
                _Name = value;
            }
        }

        public object value { get { return _Default; } set { _Default = (Float4)value; _Default.UpdateColor(); } }
        public int[] indexes { get; set; }
        public Float4Property(string Name, Float4 Value, int matIndex, int propIndex)
        {
            indexes = new int[2];
            name = Name;
            value = Value;
            indexes[0] = matIndex;
            indexes[1] = propIndex;
        }

        public int GetPropHeaderSize()
        {
            return 24;
        }

        public int GetSize()
        {
            return 16;
        }

        public void Export(BinaryWriter bw, ref long propHeadOff, ref long propOff, long basePropOff, long stringTableOff, List<int> strTableOffs)
        {
            var innerPropOff = (uint)(propOff - basePropOff);
            bw.BaseStream.Seek(propHeadOff, SeekOrigin.Begin);
            bw.Write(stringTableOff + strTableOffs[NameOffsetIndex]);
            bw.Write(Murmur3Hash(Encoding.Unicode.GetBytes(name)));
            bw.Write(Murmur3Hash(Encoding.ASCII.GetBytes(name))); bw.Write(innerPropOff);
            bw.Write(4);

            propHeadOff += GetPropHeaderSize();

            bw.BaseStream.Seek(propOff, SeekOrigin.Begin);
            bw.Write(_Default.x);
            bw.Write(_Default.y);
            bw.Write(_Default.z);
            bw.Write(_Default.w);
            propOff += GetSize();
        }
    }
}