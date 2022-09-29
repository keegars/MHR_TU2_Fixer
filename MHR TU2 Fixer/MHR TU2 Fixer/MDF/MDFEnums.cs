using System;

namespace MHR_TU2_Fixer.MDF
{
    public static class MDFEnums
    {
        [Flags]
        public enum AlphaFlags
        {
            None = 0,
            BaseTwoSideEnable = 1,
            BaseAlphaTestEnable = 2,
            ShadowCastDisable = 4,
            VertexShaderUsed = 8,
            EmissiveUsed = 16,
            TessellationEnable = 32,
            EnableIgnoreDepth = 64,
            AlphaMaskUsed = 128
        }

        [Flags]
        public enum Flags2
        {
            None = 0,
            ForcedTwoSideEnable = 1,
            TwoSideEnable = 2
        }
        [Flags]
        public enum Flags3
        {
            None = 0,
            RoughTransparentEnable = 1,
            ForcedAlphaTestEnable = 2,
            AlphaTestEnable = 4,
            SSSProfileUsed = 8,
            EnableStencilPriority = 16,
            RequireDualQuaternion = 32,
            PixelDepthOffsetUsed = 64,
            NoRayTracing = 128
        }

        public enum ShadingType
        {
            Standard = 0,
            Decal = 1,
            DecalWithMetallic = 2,
            DecalNRMR = 3,
            Transparent = 4,
            Distortion = 5,
            PrimitiveMesh = 6,
            PrimitiveSolidMesh = 7,
            Water = 8,
            SpeedTree = 9,
            GUI = 10,
            GUIMesh = 11,
            GUIMeshTransparent = 12,
            ExpensiveTransparent = 13,
            Forward = 14,
            RenderTarget = 15,
            PostProcess = 16,
            PrimitiveMaterial = 17,
            PrimitiveSolidMaterial = 18,
            SpineMaterial = 19
        }

        public enum MDFConversion
        {
            Merge,
            MergeAndAddMissingProperties,
            MergeWithFlagsAndAddMissingProperties
        }
    }
}