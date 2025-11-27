using System.Collections.Concurrent;
using System.Text;

namespace DKW.TransactionExtractor.EncodingProviders;

public sealed class PdfAliasEncodingProvider : EncodingProvider
{
    private static readonly Encoding WinAnsi = Encoding.GetEncoding(1252);
    private static readonly Encoding MacRoman = Encoding.GetEncoding(10000);

    // Alias map for known PDF and common encoding names
    private static readonly Dictionary<String, Encoding> AliasMap =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "StandardEncoding", WinAnsi },
            { "WinAnsiEncoding", WinAnsi },
            { "AdobeStandardEncoding", WinAnsi },
            { "MacRomanEncoding", MacRoman },
            { "PDFDocEncoding", WinAnsi },

            // common canonical names -> prefer built-in properties where available
            { "ISO-8859-1", Encoding.Latin1 },
            { "ISO8859-1", Encoding.Latin1 },
            { "latin1", Encoding.Latin1 },
            { "Latin-1", Encoding.Latin1 },
            { "Windows-1252", WinAnsi },
            { "1252", WinAnsi }
        };

    // Cache for resolved encodings by name (case-insensitive)
    private static readonly ConcurrentDictionary<String, Encoding?> Cache = new(StringComparer.OrdinalIgnoreCase);

    // Async-friendly reentrancy guard to prevent infinite recursion when calling Encoding.GetEncoding
    private static readonly AsyncLocal<Boolean> InResolve = new();

    static PdfAliasEncodingProvider()
    {
        // Pre-populate cache with alias map to avoid runtime lookups that could re-enter provider
        foreach (var kv in AliasMap)
        {
            Cache.TryAdd(kv.Key, kv.Value);
        }
    }

    public override Encoding? GetEncoding(String name)
    {
        if (String.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return Cache.GetOrAdd(name.Trim(), ResolveEncoding);
    }

    public override Encoding? GetEncoding(Int32 codePage)
    {
        // Avoid re-entry into Encoding.GetEncoding when we're already resolving to prevent recursion
        if (InResolve.Value)
        {
            return null;
        }

        try
        {
            InResolve.Value = true;
            return Encoding.GetEncoding(codePage);
        }
        catch
        {
            return null;
        }
        finally
        {
            InResolve.Value = false;
        }
    }

    private static Encoding? ResolveEncoding(String name)
    {
        if (InResolve.Value)
        {
            return null; // guard against re-entrancy
        }

        try
        {
            InResolve.Value = true;

            // fast alias lookup
            if (AliasMap.TryGetValue(name, out var enc))
            {
                return enc;
            }

            // Try runtime lookup as fallback. Guard prevents the provider from being called recursively.
            try
            {
                return Encoding.GetEncoding(name);
            }
            catch
            {
                return null;
            }
        }
        finally
        {
            InResolve.Value = false;
        }
    }
}
