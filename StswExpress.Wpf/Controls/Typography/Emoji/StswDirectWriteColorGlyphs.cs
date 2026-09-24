using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

namespace StswExpress.Wpf;

/// <summary>
/// Minimal DirectWrite interop used to decompose COLR v0 glyph runs into their colored layers.
/// </summary>
internal static unsafe class StswDirectWriteColorGlyphs
{
    private const int DWriteENoColor = unchecked((int)0x8898500C);
    private static readonly Guid IidDWriteFactory = new("B859EE5A-D838-4B5B-A2E8-1ADC7D93DB48");
    private static readonly Guid IidDWriteFactory2 = new("0439FC60-CA44-4994-8DEE-3A9AF7B732EC");

    private static readonly object FactoryLock = new();
    private static nint _factory;
    private static nint _factory2;
    private static bool _factoryInitialized;

    /// <summary>
    /// Uses DirectWrite to translate a WPF glyph run into COLR v0 color layers.
    /// </summary>
    public static bool TryTranslate(GlyphRun glyphRun, out IReadOnlyList<ColorGlyphLayer> layers)
    {
        layers = Array.Empty<ColorGlyphLayer>();
        if (glyphRun.GlyphIndices.Count == 0)
        {
            return false;
        }

        if (!EnsureFactory())
        {
            return false;
        }

        string? fontPath = GetFontPath(glyphRun.GlyphTypeface);
        if (string.IsNullOrWhiteSpace(fontPath) || !File.Exists(fontPath))
        {
            return false;
        }

        nint fontFile = 0;
        nint fontFace = 0;
        nint enumerator = 0;

        try
        {
            if (!TryCreateFontFace(fontPath, out fontFile, out fontFace))
            {
                return false;
            }

            int count = glyphRun.GlyphIndices.Count;
            var glyphIndices = new ushort[count];
            var glyphAdvances = new float[count];
            var glyphOffsets = new DWriteGlyphOffset[count];
            var hasOffsets = glyphRun.GlyphOffsets is { Count: > 0 };

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
                    FontEmSize = (float)glyphRun.FontRenderingEmSize,
                    GlyphCount = (uint)count,
                    GlyphIndices = pGlyphIndices,
                    GlyphAdvances = pGlyphAdvances,
                    GlyphOffsets = hasOffsets ? pGlyphOffsets : null,
                    IsSideways = glyphRun.IsSideways ? 1 : 0,
                    BidiLevel = (uint)glyphRun.BidiLevel,
                };

                void* pEnumerator = null;
                int hr = TranslateColorGlyphRun(
                    _factory2,
                    (float)glyphRun.BaselineOrigin.X,
                    (float)glyphRun.BaselineOrigin.Y,
                    &nativeRun,
                    &pEnumerator);

                if (hr == DWriteENoColor)
                {
                    return false;
                }

                if (hr < 0 || pEnumerator == null)
                {
                    return false;
                }

                enumerator = (nint)pEnumerator;
            }

            var result = new List<ColorGlyphLayer>();
            const int maxLayers = 1024;

            while (result.Count < maxLayers)
            {
                int hasRun = 0;
                int moveHr = MoveNext(enumerator, &hasRun);
                if (moveHr < 0 || hasRun == 0)
                    break;

                DWriteColorGlyphRun* nativeColorRun = null;
                int currentHr = GetCurrentRun(enumerator, &nativeColorRun);
                if (currentHr < 0 || nativeColorRun == null)
                    break;

                uint glyphCount = nativeColorRun->GlyphRun.GlyphCount;
                if (glyphCount == 0 || glyphCount > int.MaxValue)
                    continue;

                var layerIndices = new ushort[(int)glyphCount];
                var layerAdvances = new double[(int)glyphCount];
                Point[]? layerOffsets = nativeColorRun->GlyphRun.GlyphOffsets == null
                    ? null
                    : new Point[(int)glyphCount];

                for (var i = 0; i < (int)glyphCount; i++)
                {
                    layerIndices[i] = nativeColorRun->GlyphRun.GlyphIndices[i];
                    if (nativeColorRun->GlyphRun.GlyphAdvances != null)
                        layerAdvances[i] = nativeColorRun->GlyphRun.GlyphAdvances[i];

                    if (layerOffsets is not null)
                    {
                        DWriteGlyphOffset nativeOffset = nativeColorRun->GlyphRun.GlyphOffsets[i];
                        layerOffsets[i] = new Point(nativeOffset.AdvanceOffset, nativeOffset.AscenderOffset);
                    }
                }

                bool useForeground = nativeColorRun->PaletteIndex == ushort.MaxValue;
                Color color = Color.FromArgb(
                    ToByte(nativeColorRun->RunColor.A),
                    ToByte(nativeColorRun->RunColor.R),
                    ToByte(nativeColorRun->RunColor.G),
                    ToByte(nativeColorRun->RunColor.B));

                result.Add(new ColorGlyphLayer(
                    layerIndices,
                    layerAdvances,
                    layerOffsets,
                    new Point(nativeColorRun->BaselineOriginX, nativeColorRun->BaselineOriginY),
                    nativeColorRun->GlyphRun.IsSideways != 0,
                    (int)nativeColorRun->GlyphRun.BidiLevel,
                    nativeColorRun->GlyphRun.FontEmSize,
                    color,
                    useForeground));
            }

            if (result.Count == 0)
            {
                return false;
            }

            layers = result;
            return true;
        }
        catch
        {
            layers = Array.Empty<ColorGlyphLayer>();
            return false;
        }
        finally
        {
            Release(enumerator);
            Release(fontFace);
            Release(fontFile);
        }
    }

    private static bool EnsureFactory()
    {
        if (_factoryInitialized)
            return _factory2 != 0;

        lock (FactoryLock)
        {
            if (_factoryInitialized)
                return _factory2 != 0;

            nint factory = 0;
            nint factory2 = 0;

            try
            {
                Guid iidFactory = IidDWriteFactory;
                int hr = DWriteCreateFactory(0, ref iidFactory, out factory);
                if (hr < 0 || factory == 0)
                    return false;

                Guid iidFactory2 = IidDWriteFactory2;
                void* pFactory2 = null;
                hr = QueryInterface(factory, &iidFactory2, &pFactory2);
                if (hr < 0 || pFactory2 == null)
                    return false;

                factory2 = (nint)pFactory2;
                _factory = factory;
                _factory2 = factory2;
                factory = 0;
                factory2 = 0;
                return true;
            }
            finally
            {
                Release(factory2);
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
        if (uri is null)
            return null;

        if (uri.IsFile)
            return uri.LocalPath;

        return null;
    }

    private static byte ToByte(float value)
    {
        value = Math.Clamp(value, 0f, 1f);
        return (byte)(value * 255f + 0.5f);
    }

    private static int QueryInterface(nint unknown, Guid* iid, void** result)
    {
        if (unknown == 0)
            return unchecked((int)0x80004003); // E_POINTER

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

    private static int TranslateColorGlyphRun(nint factory2, float baselineX, float baselineY, DWriteGlyphRun* glyphRun, void** enumerator)
    {
        void** vtable = *(void***)factory2;
        return ((delegate* unmanaged<void*, float, float, DWriteGlyphRun*, void*, int, void*, uint, void**, int>)vtable[28])(
            (void*)factory2,
            baselineX,
            baselineY,
            glyphRun,
            null,
            0, // DWRITE_MEASURING_MODE_NATURAL
            null,
            0,
            enumerator);
    }

    private static int MoveNext(nint enumerator, int* hasRun)
    {
        void** vtable = *(void***)enumerator;
        return ((delegate* unmanaged<void*, int*, int>)vtable[3])((void*)enumerator, hasRun);
    }

    private static int GetCurrentRun(nint enumerator, DWriteColorGlyphRun** run)
    {
        void** vtable = *(void***)enumerator;
        return ((delegate* unmanaged<void*, DWriteColorGlyphRun**, int>)vtable[4])((void*)enumerator, run);
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
    private struct DWriteColorF
    {
        public float R;
        public float G;
        public float B;
        public float A;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DWriteColorGlyphRun
    {
        public DWriteGlyphRun GlyphRun;
        public void* GlyphRunDescription;
        public float BaselineOriginX;
        public float BaselineOriginY;
        public DWriteColorF RunColor;
        public ushort PaletteIndex;
    }

    internal sealed record ColorGlyphLayer(
        ushort[] GlyphIndices,
        double[] AdvanceWidths,
        Point[]? GlyphOffsets,
        Point BaselineOrigin,
        bool IsSideways,
        int BidiLevel,
        double FontRenderingEmSize,
        Color Color,
        bool UseForegroundColor);
}
