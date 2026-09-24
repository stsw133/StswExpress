using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StswExpress.Wpf;

/// <summary>
/// Raster fallback for modern color-font formats (notably COLR v1) using DirectWrite's
/// color-aware bitmap render target. The normal COLR v0 path remains vector-based.
/// </summary>
internal static unsafe class StswDirectWriteColorBitmapRenderer
{
    private static readonly Guid IidDWriteFactory = new("B859EE5A-D838-4B5B-A2E8-1ADC7D93DB48");
    private static readonly Guid IidDWriteBitmapRenderTarget3 = new("AEEC37DB-C337-40F1-8E2A-9A41B167B238");

    private static readonly object FactoryLock = new();
    private static nint _factory;
    private static bool _factoryInitialized;

    public static bool TryRender(
        GlyphRun glyphRun,
        Color foreground,
        double pixelsPerDip,
        out BitmapSource? bitmap)
    {
        bitmap = null;

        Log(
            $"TryRender START fontUri='{glyphRun.GlyphTypeface.FontUri}', " +
            $"glyphs=[{string.Join(",", glyphRun.GlyphIndices)}], em={glyphRun.FontRenderingEmSize:0.###}, " +
            $"pixelsPerDip={pixelsPerDip:0.###}");

        if (glyphRun.GlyphIndices.Count == 0 ||
            pixelsPerDip <= 0d ||
            double.IsNaN(pixelsPerDip) ||
            double.IsInfinity(pixelsPerDip))
        {
            Log("TryRender STOP: invalid input.");
            return false;
        }

        if (!EnsureFactory())
        {
            Log("TryRender STOP: IDWriteFactory unavailable.");
            return false;
        }

        string? fontPath = GetFontPath(glyphRun.GlyphTypeface);
        if (string.IsNullOrWhiteSpace(fontPath) || !File.Exists(fontPath))
        {
            Log($"TryRender STOP: invalid font path '{fontPath ?? "<null>"}'.");
            return false;
        }

        nint fontFile = 0;
        nint fontFace = 0;
        nint gdiInterop = 0;
        nint renderTarget = 0;
        nint renderTarget3 = 0;
        nint renderingParams = 0;

        try
        {
            if (!TryCreateFontFace(fontPath, out fontFile, out fontFace))
            {
                Log("TryRender STOP: TryCreateFontFace failed.");
                return false;
            }

            void* pGdiInterop = null;
            int hr = GetGdiInterop(_factory, &pGdiInterop);
            Log($"Factory.GetGdiInterop hr=0x{hr:X8}, ptr=0x{(nint)pGdiInterop:X}");
            if (hr < 0 || pGdiInterop == null)
                return false;

            gdiInterop = (nint)pGdiInterop;

            double emSize = glyphRun.FontRenderingEmSize;
            double advance = 0d;
            for (var i = 0; i < glyphRun.AdvanceWidths.Count; i++)
                advance += Math.Abs(glyphRun.AdvanceWidths[i]);

            // Generous surface; it is cropped to the actual affected pixels after rendering.
            double paddingDip = Math.Max(4d, emSize);
            double widthDip = Math.Max(advance, emSize * 1.5d) + paddingDip * 2d;
            double heightDip = emSize * 3d + paddingDip * 2d;

            uint widthPx = (uint)Math.Clamp(Math.Ceiling(widthDip * pixelsPerDip), 8d, 4096d);
            uint heightPx = (uint)Math.Clamp(Math.Ceiling(heightDip * pixelsPerDip), 8d, 4096d);

            void* pRenderTarget = null;
            hr = CreateBitmapRenderTarget(gdiInterop, widthPx, heightPx, &pRenderTarget);
            Log($"CreateBitmapRenderTarget {widthPx}x{heightPx} hr=0x{hr:X8}, ptr=0x{(nint)pRenderTarget:X}");
            if (hr < 0 || pRenderTarget == null)
                return false;

            renderTarget = (nint)pRenderTarget;

            Guid iidTarget3 = IidDWriteBitmapRenderTarget3;
            void* pRenderTarget3 = null;
            hr = QueryInterface(renderTarget, &iidTarget3, &pRenderTarget3);
            Log($"QueryInterface(IDWriteBitmapRenderTarget3) hr=0x{hr:X8}, ptr=0x{(nint)pRenderTarget3:X}");
            if (hr < 0 || pRenderTarget3 == null)
            {
                Log("TryRender STOP: IDWriteBitmapRenderTarget3 unavailable.");
                return false;
            }

            renderTarget3 = (nint)pRenderTarget3;

            void* pRenderingParams = null;
            hr = CreateRenderingParams(_factory, &pRenderingParams);
            Log($"Factory.CreateRenderingParams hr=0x{hr:X8}, ptr=0x{(nint)pRenderingParams:X}");
            if (hr < 0 || pRenderingParams == null)
                return false;

            renderingParams = (nint)pRenderingParams;

            hr = SetPixelsPerDip(renderTarget, (float)pixelsPerDip);
            Log($"SetPixelsPerDip hr=0x{hr:X8}");
            if (hr < 0)
                return false;

            DWriteBitmapData bitmapData = default;
            hr = GetBitmapData(renderTarget3, &bitmapData);
            Log($"GetBitmapData(before) hr=0x{hr:X8}, size={bitmapData.Width}x{bitmapData.Height}, pixels=0x{(nint)bitmapData.Pixels:X}");
            if (hr < 0 || bitmapData.Pixels == null || bitmapData.Width == 0 || bitmapData.Height == 0)
                return false;

            nuint pixelCount = (nuint)bitmapData.Width * bitmapData.Height;
            new Span<uint>(bitmapData.Pixels, checked((int)pixelCount)).Clear();

            int count = glyphRun.GlyphIndices.Count;
            var glyphIndices = new ushort[count];
            var glyphAdvances = new float[count];
            var glyphOffsets = new DWriteGlyphOffset[count];
            bool hasOffsets = glyphRun.GlyphOffsets is { Count: > 0 };

            for (var i = 0; i < count; i++)
            {
                glyphIndices[i] = glyphRun.GlyphIndices[i];
                glyphAdvances[i] = i < glyphRun.AdvanceWidths.Count
                    ? (float)glyphRun.AdvanceWidths[i]
                    : 0f;

                if (hasOffsets && i < glyphRun.GlyphOffsets.Count)
                {
                    Point offset = glyphRun.GlyphOffsets[i];
                    glyphOffsets[i] = new DWriteGlyphOffset
                    {
                        AdvanceOffset = (float)offset.X,
                        AscenderOffset = (float)offset.Y,
                    };
                }
            }

            fixed (ushort* pGlyphIndices = glyphIndices)
            fixed (float* pGlyphAdvances = glyphAdvances)
            fixed (DWriteGlyphOffset* pGlyphOffsets = glyphOffsets)
            {
                var nativeRun = new DWriteGlyphRun
                {
                    FontFace = (void*)fontFace,
                    FontEmSize = (float)emSize,
                    GlyphCount = (uint)count,
                    GlyphIndices = pGlyphIndices,
                    GlyphAdvances = pGlyphAdvances,
                    GlyphOffsets = hasOffsets ? pGlyphOffsets : null,
                    IsSideways = glyphRun.IsSideways ? 1 : 0,
                    BidiLevel = (uint)glyphRun.BidiLevel,
                };

                float baselineX = (float)paddingDip;
                float baselineY = (float)(paddingDip + glyphRun.GlyphTypeface.Baseline * emSize);

                DWriteRect blackBox = default;
                uint textColor = ToColorRef(foreground);

                hr = DrawGlyphRunWithColorSupport(
                    renderTarget3,
                    baselineX,
                    baselineY,
                    &nativeRun,
                    renderingParams,
                    textColor,
                    &blackBox);

                Log(
                    $"DrawGlyphRunWithColorSupport hr=0x{hr:X8}, " +
                    $"blackBox=({blackBox.Left},{blackBox.Top})-({blackBox.Right},{blackBox.Bottom})");

                if (hr < 0)
                    return false;
            }

            bitmapData = default;
            hr = GetBitmapData(renderTarget3, &bitmapData);
            if (hr < 0 || bitmapData.Pixels == null)
                return false;

            if (!TryGetVisiblePixelBounds(bitmapData, out int left, out int top, out int right, out int bottom))
            {
                Log("TryRender STOP: bitmap contains no visible pixels.");
                return false;
            }

            int cropWidth = right - left;
            int cropHeight = bottom - top;
            if (cropWidth <= 0 || cropHeight <= 0)
                return false;

            int stride = checked(cropWidth * 4);
            var pixels = new byte[checked(stride * cropHeight)];

            int bitmapWidthPx = checked((int)bitmapData.Width);
            for (var y = 0; y < cropHeight; y++)
            {
                uint* sourceRow = bitmapData.Pixels + ((top + y) * bitmapWidthPx) + left;
                fixed (byte* destination = &pixels[y * stride])
                {
                    Buffer.MemoryCopy(sourceRow, destination, stride, stride);
                }
            }

            RepairMissingAlpha(pixels);

            double dpi = 96d * pixelsPerDip;
            var result = BitmapSource.Create(
                cropWidth,
                cropHeight,
                dpi,
                dpi,
                PixelFormats.Pbgra32,
                null,
                pixels,
                stride);

            if (result.CanFreeze)
                result.Freeze();

            bitmap = result;
            Log($"TryRender SUCCESS: crop={cropWidth}x{cropHeight}");
            return true;
        }
        catch (Exception ex)
        {
            StswLog.WriteException(ex, StswInfoType.Error, "StswDirectWriteColorBitmapRenderer.TryRender");
            bitmap = null;
            return false;
        }
        finally
        {
            Release(renderingParams);
            Release(renderTarget3);
            Release(renderTarget);
            Release(gdiInterop);
            Release(fontFace);
            Release(fontFile);
        }
    }

    private static bool EnsureFactory()
    {
        if (_factoryInitialized)
            return _factory != 0;

        lock (FactoryLock)
        {
            if (_factoryInitialized)
                return _factory != 0;

            nint factory = 0;
            try
            {
                Guid iidFactory = IidDWriteFactory;
                int hr = DWriteCreateFactory(0, ref iidFactory, out factory);
                Log($"DWriteCreateFactory hr=0x{hr:X8}, factory=0x{factory:X}");
                if (hr < 0 || factory == 0)
                    return false;

                _factory = factory;
                factory = 0;
                return true;
            }
            finally
            {
                Release(factory);
                _factoryInitialized = true;
            }
        }
    }

    private static bool TryCreateFontFace(string fontPath, out nint fontFile, out nint fontFace)
    {
        fontFile = 0;
        fontFace = 0;

        fixed (char* pPath = fontPath)
        {
            void* pFontFile = null;
            int hr = CreateFontFileReference(_factory, pPath, &pFontFile);
            if (hr < 0 || pFontFile == null)
                return false;

            fontFile = (nint)pFontFile;
        }

        int supported = 0;
        int fontFileType = 0;
        int fontFaceType = 0;
        uint numberOfFaces = 0;
        int analyzeHr = AnalyzeFontFile(fontFile, &supported, &fontFileType, &fontFaceType, &numberOfFaces);
        if (analyzeHr < 0 || supported == 0 || numberOfFaces == 0)
            return false;

        void* localFontFile = (void*)fontFile;
        void* pFontFace = null;
        int faceHr = CreateFontFace(
            _factory,
            fontFaceType,
            1,
            &localFontFile,
            0,
            0,
            &pFontFace);

        if (faceHr < 0 || pFontFace == null)
            return false;

        fontFace = (nint)pFontFace;
        return true;
    }

    private static string? GetFontPath(GlyphTypeface glyphTypeface)
    {
        Uri? uri = glyphTypeface.FontUri;
        return uri is { IsFile: true } ? uri.LocalPath : null;
    }

    private static void Log(string message)
        => StswLog.Write(StswInfoType.Debug, $"[EmojiDirectWriteBitmap] {message}");

    private static uint ToColorRef(Color color)
        => (uint)(color.R | (color.G << 8) | (color.B << 16));

    private static bool TryGetVisiblePixelBounds(
        DWriteBitmapData data,
        out int left,
        out int top,
        out int right,
        out int bottom)
    {
        left = checked((int)data.Width);
        top = checked((int)data.Height);
        right = 0;
        bottom = 0;

        bool found = false;
        int width = checked((int)data.Width);
        int height = checked((int)data.Height);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                uint pixel = data.Pixels[(y * width) + x];
                if (pixel == 0)
                    continue;

                found = true;
                if (x < left) left = x;
                if (y < top) top = y;
                if (x + 1 > right) right = x + 1;
                if (y + 1 > bottom) bottom = y + 1;
            }
        }

        return found;
    }

    private static void RepairMissingAlpha(byte[] pixels)
    {
        bool hasAnyAlpha = false;
        bool hasRgbWithoutAlpha = false;

        for (var i = 0; i + 3 < pixels.Length; i += 4)
        {
            byte a = pixels[i + 3];
            if (a != 0)
                hasAnyAlpha = true;
            else if (pixels[i] != 0 || pixels[i + 1] != 0 || pixels[i + 2] != 0)
                hasRgbWithoutAlpha = true;
        }

        if (hasAnyAlpha || !hasRgbWithoutAlpha)
            return;

        for (var i = 0; i + 3 < pixels.Length; i += 4)
        {
            if (pixels[i] != 0 || pixels[i + 1] != 0 || pixels[i + 2] != 0)
                pixels[i + 3] = 255;
        }
    }

    private static int QueryInterface(nint unknown, Guid* iid, void** result)
    {
        if (unknown == 0)
            return unchecked((int)0x80004003);

        void** vtable = *(void***)unknown;
        return ((delegate* unmanaged<void*, Guid*, void**, int>)vtable[0])((void*)unknown, iid, result);
    }

    private static uint Release(nint unknown)
    {
        if (unknown == 0)
            return 0;

        void** vtable = *(void***)unknown;
        return ((delegate* unmanaged<void*, uint>)vtable[2])((void*)unknown);
    }

    private static int CreateFontFileReference(nint factory, char* filePath, void** fontFile)
    {
        void** vtable = *(void***)factory;
        return ((delegate* unmanaged<void*, char*, void*, void**, int>)vtable[7])(
            (void*)factory,
            filePath,
            null,
            fontFile);
    }

    private static int CreateFontFace(
        nint factory,
        int fontFaceType,
        uint numberOfFiles,
        void** fontFiles,
        uint faceIndex,
        int simulations,
        void** fontFace)
    {
        void** vtable = *(void***)factory;
        return ((delegate* unmanaged<void*, int, uint, void**, uint, int, void**, int>)vtable[9])(
            (void*)factory,
            fontFaceType,
            numberOfFiles,
            fontFiles,
            faceIndex,
            simulations,
            fontFace);
    }

    private static int AnalyzeFontFile(nint fontFile, int* supported, int* fileType, int* faceType, uint* numberOfFaces)
    {
        void** vtable = *(void***)fontFile;
        return ((delegate* unmanaged<void*, int*, int*, int*, uint*, int>)vtable[5])(
            (void*)fontFile,
            supported,
            fileType,
            faceType,
            numberOfFaces);
    }

    private static int GetGdiInterop(nint factory, void** gdiInterop)
    {
        void** vtable = *(void***)factory;
        return ((delegate* unmanaged<void*, void**, int>)vtable[17])((void*)factory, gdiInterop);
    }

    private static int CreateRenderingParams(nint factory, void** renderingParams)
    {
        void** vtable = *(void***)factory;
        return ((delegate* unmanaged<void*, void**, int>)vtable[10])((void*)factory, renderingParams);
    }

    private static int CreateBitmapRenderTarget(nint gdiInterop, uint width, uint height, void** renderTarget)
    {
        void** vtable = *(void***)gdiInterop;
        return ((delegate* unmanaged<void*, void*, uint, uint, void**, int>)vtable[7])(
            (void*)gdiInterop,
            null,
            width,
            height,
            renderTarget);
    }

    private static int SetPixelsPerDip(nint renderTarget, float pixelsPerDip)
    {
        void** vtable = *(void***)renderTarget;
        return ((delegate* unmanaged<void*, float, int>)vtable[6])((void*)renderTarget, pixelsPerDip);
    }

    private static int GetBitmapData(nint renderTarget3, DWriteBitmapData* bitmapData)
    {
        void** vtable = *(void***)renderTarget3;
        return ((delegate* unmanaged<void*, DWriteBitmapData*, int>)vtable[13])((void*)renderTarget3, bitmapData);
    }

    private static int DrawGlyphRunWithColorSupport(
        nint renderTarget3,
        float baselineX,
        float baselineY,
        DWriteGlyphRun* glyphRun,
        nint renderingParams,
        uint textColor,
        DWriteRect* blackBox)
    {
        void** vtable = *(void***)renderTarget3;
        return ((delegate* unmanaged<void*, float, float, int, DWriteGlyphRun*, void*, uint, uint, DWriteRect*, int>)vtable[16])(
            (void*)renderTarget3,
            baselineX,
            baselineY,
            0,
            glyphRun,
            (void*)renderingParams,
            textColor,
            0,
            blackBox);
    }

    [DllImport("dwrite.dll", ExactSpelling = true)]
    private static extern int DWriteCreateFactory(int factoryType, ref Guid iid, out nint factory);

    [StructLayout(LayoutKind.Sequential)]
    private struct DWriteGlyphOffset
    {
        public float AdvanceOffset;
        public float AscenderOffset;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DWriteGlyphRun
    {
        public void* FontFace;
        public float FontEmSize;
        public uint GlyphCount;
        public ushort* GlyphIndices;
        public float* GlyphAdvances;
        public DWriteGlyphOffset* GlyphOffsets;
        public int IsSideways;
        public uint BidiLevel;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DWriteBitmapData
    {
        public uint Width;
        public uint Height;
        public uint* Pixels;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DWriteRect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
