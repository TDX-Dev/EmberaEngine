using System;
using System.Collections.Generic;
using ImGuiNET;
using OpenTK.Graphics.OpenGL;

namespace EmberaEngine.Engine.Rendering
{
    class GraphicsState
    {

        public static void ErrorCheck()
        {
            Console.WriteLine(GL.GetError());
        }

        public static void ClearColor(int r, int g, int b, int a)
        {
            GL.ClearColor(r, g, b, a);
        }
        public static void Clear(bool color = true, bool depth = false, bool stencil = false)
        {
            ClearBufferMask clearMask = color ? ClearBufferMask.ColorBufferBit : ClearBufferMask.None;

            clearMask |= depth ? ClearBufferMask.DepthBufferBit : ClearBufferMask.None;
            clearMask |= stencil ? ClearBufferMask.StencilBufferBit : ClearBufferMask.None;

            GL.Clear(clearMask);
        }

        public static void SetViewport(int x, int y, int w, int h)
        {
            GL.Viewport(x, y, w, h);
        }

        public static void ClearTextureBinding2D()
        {
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public static void ClearFrameBufferBinding()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public static void SetBlending(bool blend)
        {
            GL.Enable(EnableCap.Blend);
        }

        public static void SetBlendingFunc(BlendingFactor blendingFactor, BlendingFactor factor)
        {
            GL.BlendFunc((OpenTK.Graphics.OpenGL.BlendingFactor)blendingFactor, (OpenTK.Graphics.OpenGL.BlendingFactor)factor);
        }

        public static void SetDepthTest(bool depthTest)
        {
            if (depthTest)
            {
                GL.Enable(EnableCap.DepthTest);
            }
            else
            {
                GL.Disable(EnableCap.DepthTest);
            }
        }

        public static void SetCulling(bool culling)
        {
            if (culling)
            {
                GL.Enable(EnableCap.CullFace);
            }
            else
            {
                GL.Disable(EnableCap.CullFace);
            }
        }

        public static void SetPixelStoreI(PixelStoreParameter ps, int val)
        {
            GL.PixelStore((OpenTK.Graphics.OpenGL.PixelStoreParameter)ps, val);
        }

    }

    public enum PixelStoreParameter
    {
        //
        // Summary:
        //     [requires: v1.0] Original was GL_UNPACK_SWAP_BYTES = 0x0CF0
        UnpackSwapBytes = 3312,
        //
        // Summary:
        //     [requires: v1.0] Original was GL_UNPACK_LSB_FIRST = 0x0CF1
        UnpackLsbFirst = 3313,
        //
        // Summary:
        //     [requires: v1.0] Original was GL_UNPACK_ROW_LENGTH = 0x0CF2
        UnpackRowLength = 3314,
        //
        // Summary:
        //     Original was GL_UNPACK_ROW_LENGTH_EXT = 0x0CF2
        UnpackRowLengthExt = 3314,
        //
        // Summary:
        //     [requires: v1.0] Original was GL_UNPACK_SKIP_ROWS = 0x0CF3
        UnpackSkipRows = 3315,
        //
        // Summary:
        //     Original was GL_UNPACK_SKIP_ROWS_EXT = 0x0CF3
        UnpackSkipRowsExt = 3315,
        //
        // Summary:
        //     [requires: v1.0] Original was GL_UNPACK_SKIP_PIXELS = 0x0CF4
        UnpackSkipPixels = 3316,
        //
        // Summary:
        //     Original was GL_UNPACK_SKIP_PIXELS_EXT = 0x0CF4
        UnpackSkipPixelsExt = 3316,
        //
        // Summary:
        //     [requires: v1.0] Original was GL_UNPACK_ALIGNMENT = 0x0CF5
        UnpackAlignment = 3317,
        //
        // Summary:
        //     [requires: v1.0] Original was GL_PACK_SWAP_BYTES = 0x0D00
        PackSwapBytes = 3328,
        //
        // Summary:
        //     [requires: v1.0] Original was GL_PACK_LSB_FIRST = 0x0D01
        PackLsbFirst = 3329,
        //
        // Summary:
        //     [requires: v1.0] Original was GL_PACK_ROW_LENGTH = 0x0D02
        PackRowLength = 3330,
        //
        // Summary:
        //     [requires: v1.0] Original was GL_PACK_SKIP_ROWS = 0x0D03
        PackSkipRows = 3331,
        //
        // Summary:
        //     [requires: v1.0] Original was GL_PACK_SKIP_PIXELS = 0x0D04
        PackSkipPixels = 3332,
        //
        // Summary:
        //     [requires: v1.0] Original was GL_PACK_ALIGNMENT = 0x0D05
        PackAlignment = 3333,
        //
        // Summary:
        //     [requires: v1.2] Original was GL_PACK_SKIP_IMAGES = 0x806B
        PackSkipImages = 32875,
        //
        // Summary:
        //     [requires: EXT_texture3D] Original was GL_PACK_SKIP_IMAGES_EXT = 0x806B
        PackSkipImagesExt = 32875,
        //
        // Summary:
        //     [requires: v1.2] Original was GL_PACK_IMAGE_HEIGHT = 0x806C
        PackImageHeight = 32876,
        //
        // Summary:
        //     [requires: EXT_texture3D] Original was GL_PACK_IMAGE_HEIGHT_EXT = 0x806C
        PackImageHeightExt = 32876,
        //
        // Summary:
        //     [requires: v1.2] Original was GL_UNPACK_SKIP_IMAGES = 0x806D
        UnpackSkipImages = 32877,
        //
        // Summary:
        //     [requires: EXT_texture3D] Original was GL_UNPACK_SKIP_IMAGES_EXT = 0x806D
        UnpackSkipImagesExt = 32877,
        //
        // Summary:
        //     [requires: v1.2] Original was GL_UNPACK_IMAGE_HEIGHT = 0x806E
        UnpackImageHeight = 32878,
        //
        // Summary:
        //     [requires: EXT_texture3D] Original was GL_UNPACK_IMAGE_HEIGHT_EXT = 0x806E
        UnpackImageHeightExt = 32878,
        //
        // Summary:
        //     [requires: SGIS_texture4D] Original was GL_PACK_SKIP_VOLUMES_SGIS = 0x8130
        PackSkipVolumesSgis = 33072,
        //
        // Summary:
        //     [requires: SGIS_texture4D] Original was GL_PACK_IMAGE_DEPTH_SGIS = 0x8131
        PackImageDepthSgis = 33073,
        //
        // Summary:
        //     [requires: SGIS_texture4D] Original was GL_UNPACK_SKIP_VOLUMES_SGIS = 0x8132
        UnpackSkipVolumesSgis = 33074,
        //
        // Summary:
        //     [requires: SGIS_texture4D] Original was GL_UNPACK_IMAGE_DEPTH_SGIS = 0x8133
        UnpackImageDepthSgis = 33075,
        //
        // Summary:
        //     [requires: SGIX_pixel_tiles] Original was GL_PIXEL_TILE_WIDTH_SGIX = 0x8140
        PixelTileWidthSgix = 33088,
        //
        // Summary:
        //     [requires: SGIX_pixel_tiles] Original was GL_PIXEL_TILE_HEIGHT_SGIX = 0x8141
        PixelTileHeightSgix = 33089,
        //
        // Summary:
        //     [requires: SGIX_pixel_tiles] Original was GL_PIXEL_TILE_GRID_WIDTH_SGIX = 0x8142
        PixelTileGridWidthSgix = 33090,
        //
        // Summary:
        //     [requires: SGIX_pixel_tiles] Original was GL_PIXEL_TILE_GRID_HEIGHT_SGIX = 0x8143
        PixelTileGridHeightSgix = 33091,
        //
        // Summary:
        //     [requires: SGIX_pixel_tiles] Original was GL_PIXEL_TILE_GRID_DEPTH_SGIX = 0x8144
        PixelTileGridDepthSgix = 33092,
        //
        // Summary:
        //     [requires: SGIX_pixel_tiles] Original was GL_PIXEL_TILE_CACHE_SIZE_SGIX = 0x8145
        PixelTileCacheSizeSgix = 33093,
        //
        // Summary:
        //     [requires: SGIX_resample] Original was GL_PACK_RESAMPLE_SGIX = 0x842E
        PackResampleSgix = 33838,
        //
        // Summary:
        //     [requires: SGIX_resample] Original was GL_UNPACK_RESAMPLE_SGIX = 0x842F
        UnpackResampleSgix = 33839,
        //
        // Summary:
        //     [requires: SGIX_subsample] Original was GL_PACK_SUBSAMPLE_RATE_SGIX = 0x85A0
        PackSubsampleRateSgix = 34208,
        //
        // Summary:
        //     [requires: SGIX_subsample] Original was GL_UNPACK_SUBSAMPLE_RATE_SGIX = 0x85A1
        UnpackSubsampleRateSgix = 34209,
        //
        // Summary:
        //     Original was GL_PACK_RESAMPLE_OML = 0x8984
        PackResampleOml = 35204,
        //
        // Summary:
        //     Original was GL_UNPACK_RESAMPLE_OML = 0x8985
        UnpackResampleOml = 35205,
        //
        // Summary:
        //     [requires: v4.2 or ARB_compressed_texture_pixel_storage] Original was GL_UNPACK_COMPRESSED_BLOCK_WIDTH
        //     = 0x9127
        UnpackCompressedBlockWidth = 37159,
        //
        // Summary:
        //     [requires: v4.2 or ARB_compressed_texture_pixel_storage] Original was GL_UNPACK_COMPRESSED_BLOCK_HEIGHT
        //     = 0x9128
        UnpackCompressedBlockHeight = 37160,
        //
        // Summary:
        //     [requires: v4.2 or ARB_compressed_texture_pixel_storage] Original was GL_UNPACK_COMPRESSED_BLOCK_DEPTH
        //     = 0x9129
        UnpackCompressedBlockDepth = 37161,
        //
        // Summary:
        //     [requires: v4.2 or ARB_compressed_texture_pixel_storage] Original was GL_UNPACK_COMPRESSED_BLOCK_SIZE
        //     = 0x912A
        UnpackCompressedBlockSize = 37162,
        //
        // Summary:
        //     [requires: v4.2 or ARB_compressed_texture_pixel_storage] Original was GL_PACK_COMPRESSED_BLOCK_WIDTH
        //     = 0x912B
        PackCompressedBlockWidth = 37163,
        //
        // Summary:
        //     [requires: v4.2 or ARB_compressed_texture_pixel_storage] Original was GL_PACK_COMPRESSED_BLOCK_HEIGHT
        //     = 0x912C
        PackCompressedBlockHeight = 37164,
        //
        // Summary:
        //     [requires: v4.2 or ARB_compressed_texture_pixel_storage] Original was GL_PACK_COMPRESSED_BLOCK_DEPTH
        //     = 0x912D
        PackCompressedBlockDepth = 37165,
        //
        // Summary:
        //     [requires: v4.2 or ARB_compressed_texture_pixel_storage] Original was GL_PACK_COMPRESSED_BLOCK_SIZE
        //     = 0x912E
        PackCompressedBlockSize = 37166
    }

    public enum BlendingFactor
    {
        //
        // Summary:
        //     [requires: v1.0 or NV_blend_equation_advanced, NV_register_combiners] Original
        //     was GL_ZERO = 0
        Zero = 0,
        //
        // Summary:
        //     [requires: v1.0] Original was GL_SRC_COLOR = 0x0300
        SrcColor = 768,
        //
        // Summary:
        //     [requires: v1.0] Original was GL_ONE_MINUS_SRC_COLOR = 0x0301
        OneMinusSrcColor = 769,
        //
        // Summary:
        //     [requires: v1.0] Original was GL_SRC_ALPHA = 0x0302
        SrcAlpha = 770,
        //
        // Summary:
        //     [requires: v1.0] Original was GL_ONE_MINUS_SRC_ALPHA = 0x0303
        OneMinusSrcAlpha = 771,
        //
        // Summary:
        //     [requires: v1.0] Original was GL_DST_ALPHA = 0x0304
        DstAlpha = 772,
        //
        // Summary:
        //     [requires: v1.0] Original was GL_ONE_MINUS_DST_ALPHA = 0x0305
        OneMinusDstAlpha = 773,
        //
        // Summary:
        //     [requires: v1.0] Original was GL_DST_COLOR = 0x0306
        DstColor = 774,
        //
        // Summary:
        //     [requires: v1.0] Original was GL_ONE_MINUS_DST_COLOR = 0x0307
        OneMinusDstColor = 775,
        //
        // Summary:
        //     [requires: v1.0] Original was GL_SRC_ALPHA_SATURATE = 0x0308
        SrcAlphaSaturate = 776,
        //
        // Summary:
        //     [requires: v1.4 or ARB_imaging] Original was GL_CONSTANT_COLOR = 0x8001
        ConstantColor = 32769,
        //
        // Summary:
        //     [requires: v1.4 or ARB_imaging] Original was GL_ONE_MINUS_CONSTANT_COLOR = 0x8002
        OneMinusConstantColor = 32770,
        //
        // Summary:
        //     [requires: v1.4 or ARB_imaging] Original was GL_CONSTANT_ALPHA = 0x8003
        ConstantAlpha = 32771,
        //
        // Summary:
        //     [requires: v1.4 or ARB_imaging] Original was GL_ONE_MINUS_CONSTANT_ALPHA = 0x8004
        OneMinusConstantAlpha = 32772,
        //
        // Summary:
        //     [requires: v1.5 or ARB_blend_func_extended] Original was GL_SRC1_ALPHA = 0x8589
        Src1Alpha = 34185,
        //
        // Summary:
        //     [requires: v3.3 or ARB_blend_func_extended] Original was GL_SRC1_COLOR = 0x88F9
        Src1Color = 35065,
        //
        // Summary:
        //     [requires: v1.0] Original was GL_ONE = 1
        One = 1
    }
}
