using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MHR_TU2_Fixer.MDF;
using static MHR_TU2_Fixer.MDF.MDFEnums;

namespace MHR_TU2_Fixer.Helpers
{
    public static class MDFHelper
    {
        public static void ConvertMDFFiles(string[] files, MDFConversion mdfConversion)
        {
            foreach (var file in files)
            {
                var newFileName = file + ".old";
                File.Move(file, newFileName);
                var createFile = File.Create(file);
                createFile.Close();

                using (var openFile = OpenFile(newFileName))
                {
                    var mdfFile = new MDFFile(newFileName, openFile);
                    var newMDFFile = Path.Combine(Path.GetDirectoryName(file), $"{Path.GetFileNameWithoutExtension(file)}.23");
                    mdfFile.Save(newMDFFile, mdfConversion);

                    openFile.Close();
                    File.Delete(newFileName);
                }
            }
        }

        public static BinaryReader OpenFile(string filePath)
        {
            return new BinaryReader(new FileStream(filePath, FileMode.Open), Encoding.Unicode);
        }

        public static void Save(this MDFFile mdfFile, string filePath, MDFConversion mdfConversion)
        {
            if (mdfFile.Materials.Count > 0)
            {
                using (var mdfBW = new BinaryWriter(new FileStream(filePath, FileMode.Create), Encoding.Unicode))
                {
                    mdfFile.Export(mdfBW, mdfConversion);
                }
            }
        }

        public static void Export(this MDFFile mdfFile, BinaryWriter bw, MDFConversion mdfConversion = MDFConversion.Merge)
        {
            //Convert old mdf2 to an already working mdf2....
            //There is a working skin, and alpha/normal body....
            //Ignore property - OCC Color as it has caused issues with the models showing as white....

            var fileName = Path.Combine(Environment.CurrentDirectory, "MDF/example/TU2/f_arm272.mdf2.23");
            var npcFaceFileName = Path.Combine(Environment.CurrentDirectory, "MDF/example/TU2/npc001_00_face.mdf2.23");
            var npcBodyFileName = Path.Combine(Environment.CurrentDirectory, "MDF/example/TU2/npc002_00_body.mdf2.23");

            var bodyDetector = "BaseDielectricMap";
            var skinDetector = "SkinMap";
            var npcEyeDetector = "Mask";
            var npcHandDetector = new List<string> { "BaseDielectricMap", "NRMR_NRRTMap", "AlphaMap" };

            //Always merge
            for (var i = 0; i < mdfFile.Materials.Count; i++)
            {
                var binary = OpenFile(fileName);
                var npcFaceBinary = OpenFile(npcFaceFileName);
                var npcBodyBinary = OpenFile(npcBodyFileName);

                var exampleMDF = new MDFFile(fileName, binary);
                var exampleNPCFaceMDF = new MDFFile(npcFaceFileName, npcFaceBinary);
                var exampleNPCBodyMDF = new MDFFile(npcBodyFileName, npcBodyBinary);

                var bodyMaterial = exampleMDF.Materials[2];
                var alphaBodyMaterial = exampleMDF.Materials[0];
                var skinMaterial = exampleMDF.Materials[4];
                var eyeMaterial = exampleNPCFaceMDF.Materials[0];
                var handMaterial = exampleNPCBodyMDF.Materials[1];

                var material = mdfFile.Materials[i];
                var isAlphaCheck = material.flags.Any(z => z.Name == "BaseAlphaTestEnable");

                if (material.ShaderType == ShadingType.Decal || material.ShaderType == ShadingType.DecalNRMR || material.ShaderType == ShadingType.DecalWithMetallic)
                {
                    npcFaceBinary.Close();
                    npcBodyBinary.Close();
                    binary.Close();
                    continue;
                }

                Material newMaterial = null;

                //Assign the correct material
                if (material.Textures.All(z=> npcHandDetector.Contains(z.name)) && mdfFile.FileName.Contains(@"\npc\"))
                {
                    newMaterial = handMaterial;
                }
                else if (material.Textures.Any(z => z.name == npcEyeDetector) && mdfFile.FileName.Contains(@"\npc\")) {
                    newMaterial = eyeMaterial;
                }
                else if (material.Textures.Any(z => z.name == skinDetector))
                {
                    newMaterial = skinMaterial;
                }
                else if (material.Textures.Any(z => z.name == bodyDetector))
                {
                    newMaterial = isAlphaCheck ? alphaBodyMaterial : bodyMaterial;
                }

                //If detected, lets merge the values...
                if (newMaterial != null)
                {
                    for (var j = 0; j < newMaterial.Textures.Count; j++)
                    {
                        //Get the current new material texture
                        var tmpMaterial = newMaterial.Textures[j];

                        //Try and find the matching texture in the old material
                        var materialTexture = material.Textures.FirstOrDefault(z => z.name == tmpMaterial.name);

                        //If match found, lets update it's path
                        if (materialTexture != null)
                        {
                            tmpMaterial.path = materialTexture.path;
                        }
                    }

                    for (var j = 0; j < newMaterial.Properties.Count; j++)
                    {
                        //Get the current new material property
                        var tmpProperty = newMaterial.Properties[j];

                        //Try and find the matching property in the old material
                        var materialProperty = material.Properties.FirstOrDefault(z => tmpProperty.name == z.name);

                        if (materialProperty != null)
                        {
                            materialProperty.indexes = tmpProperty.indexes;

                            newMaterial.Properties[j] = materialProperty;
                        }
                    }

                    if (mdfConversion == MDFConversion.MergeAndAddMissingProperties)
                    {
                        var propertiesModified = false;

                        foreach (var property in material.Properties)
                        {
                            var newProperty = newMaterial.Properties.Any(z => z.name == property.name);

                            if (!newMaterial.Properties.Any(z => z.name == property.name) && property.name != "OCC_color")
                            {
                                newMaterial.Properties.Add(property);

                                propertiesModified = true;
                            }
                        }

                        //If we added new ones, re-index the properties
                        if (propertiesModified)
                        {
                            for (var j = 0; j < newMaterial.Properties.Count; j++)
                            {
                                newMaterial.Properties[j].indexes[0] = i; //Material index
                                newMaterial.Properties[j].indexes[1] = j; //Property index
                            }
                        }
                    }

                    //Update textures, properties and flags to match
                    material.Textures = newMaterial.Textures;
                    material.Properties = newMaterial.Properties;

                    if (mdfConversion == MDFConversion.MergeWithFlagsAndAddMissingProperties)
                    {
                        material.flags = newMaterial.flags;
                    }
                }

                npcFaceBinary.Close();
                npcBodyBinary.Close();
                binary.Close();
            }

            //Where the magic happens~~~~

            bw.Write(MDFFile.Magic);
            bw.Write((short)1);
            bw.Write((short)mdfFile.Materials.Count);
            bw.Write((long)0);

            var strTableOffsets = new List<int>();
            var stringTable = mdfFile.GenerateStringTable(ref strTableOffsets);

            var materialOffset = bw.BaseStream.Position;
            while ((materialOffset % 16) != 0)
            {
                materialOffset++;
            }
            var textureOffset = materialOffset;
            for (var i = 0; i < mdfFile.Materials.Count; i++)
            {
                textureOffset += mdfFile.Materials[i].GetSize();
            }
            while ((textureOffset % 16) != 0)
            {
                textureOffset++;
            }
            var propHeadersOffset = textureOffset;
            for (var i = 0; i < mdfFile.Materials.Count; i++)
            {
                for (var j = 0; j < mdfFile.Materials[i].Textures.Count; j++)
                {
                    propHeadersOffset += mdfFile.Materials[i].Textures[j].GetSize();
                }
            }
            while ((propHeadersOffset % 16) != 0)
            {
                propHeadersOffset++;
            }
            var stringTableOffset = propHeadersOffset;
            for (var i = 0; i < mdfFile.Materials.Count; i++)
            {
                for (var j = 0; j < mdfFile.Materials[i].Properties.Count; j++)
                {
                    stringTableOffset += mdfFile.Materials[i].Properties[j].GetPropHeaderSize();
                }
            }
            while ((stringTableOffset % 16) != 0)
            {
                stringTableOffset++;
            }
            var propertiesOffset = stringTableOffset + stringTable.Count;
            while ((propertiesOffset % 16) != 0)
            {
                propertiesOffset++;
            }
            bw.BaseStream.Seek(stringTableOffset, SeekOrigin.Begin);
            for (var i = 0; i < stringTable.Count; i++)
            {
                bw.Write(stringTable[i]);
            }
            for (var i = 0; i < mdfFile.Materials.Count; i++)
            {
                mdfFile.Materials[i].Export(bw, ref materialOffset, ref textureOffset, ref propHeadersOffset, stringTableOffset, strTableOffsets, ref propertiesOffset);
            }
        }
    }
}