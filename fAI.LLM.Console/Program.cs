// gguf-info: reads and displays the header of a GGUF model file.
// Targets .NET Framework 4.8 / C# 7.3.
//
// Usage:
//   gguf-info <model.gguf> [--tensors] [--full] [--preview N]
//
// The file is treated as untrusted input: every length and count read from it
// is validated against sane limits and the remaining file size before any
// allocation, seek, or loop, and all text is escaped before being printed.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace GgufInspector
{
    internal enum GgufValueType : uint
    {
        UInt8 = 0, Int8 = 1, UInt16 = 2, Int16 = 3, UInt32 = 4, Int32 = 5,
        Float32 = 6, Bool = 7, String = 8, Array = 9, UInt64 = 10, Int64 = 11, Float64 = 12
    }

    internal sealed class MetadataEntry
    {
        public MetadataEntry(string key, string typeName, string display)
        {
            Key = key;
            TypeName = typeName;
            Display = display;
        }

        public string Key { get; private set; }
        public string TypeName { get; private set; }
        public string Display { get; private set; }
    }

    internal sealed class TensorInfo
    {
        public TensorInfo(string name, ulong[] dims, uint ggmlType, ulong offset, ulong elementCount)
        {
            Name = name;
            Dims = dims;
            GgmlType = ggmlType;
            Offset = offset;
            ElementCount = elementCount;
        }

        public string Name { get; private set; }
        public ulong[] Dims { get; private set; }
        public uint GgmlType { get; private set; }
        public ulong Offset { get; private set; }
        public ulong ElementCount { get; private set; }
    }

    internal sealed class GgufFile
    {
        public GgufFile()
        {
            Metadata = new List<MetadataEntry>();
            Tensors = new List<TensorInfo>();
            Alignment = 32;
        }

        public long FileSize { get; set; }
        public uint Version { get; set; }
        public ulong TensorCount { get; set; }
        public ulong MetadataCount { get; set; }
        public List<MetadataEntry> Metadata { get; private set; }
        public List<TensorInfo> Tensors { get; private set; }
        public ulong Alignment { get; set; }
        public ulong DataOffset { get; set; }
    }

    internal sealed class GgufParser : IDisposable
    {
        private const uint MagicLittleEndian = 0x46554747; // "GGUF"
        private const uint MagicBigEndian = 0x47475546;
        private const int MaxStringBytes = 16 * 1024 * 1024;
        private const ulong MaxMetadataCount = 1000000;
        private const ulong MaxTensorCount = 10000000;
        private const uint MaxDimensions = 8;
        private const int MaxArrayDepth = 8;

        private static readonly Dictionary<uint, string> GgmlTypeNames = new Dictionary<uint, string>
        {
            { 0, "F32" }, { 1, "F16" }, { 2, "Q4_0" }, { 3, "Q4_1" }, { 6, "Q5_0" }, { 7, "Q5_1" },
            { 8, "Q8_0" }, { 9, "Q8_1" }, { 10, "Q2_K" }, { 11, "Q3_K" }, { 12, "Q4_K" }, { 13, "Q5_K" },
            { 14, "Q6_K" }, { 15, "Q8_K" }, { 16, "IQ2_XXS" }, { 17, "IQ2_XS" }, { 18, "IQ3_XXS" },
            { 19, "IQ1_S" }, { 20, "IQ4_NL" }, { 21, "IQ3_S" }, { 22, "IQ2_S" }, { 23, "IQ4_XS" },
            { 24, "I8" }, { 25, "I16" }, { 26, "I32" }, { 27, "I64" }, { 28, "F64" }, { 29, "IQ1_M" },
            { 30, "BF16" }, { 34, "TQ1_0" }, { 35, "TQ2_0" }, { 39, "MXFP4" }
        };

        private readonly FileStream _stream;
        private readonly BinaryReader _reader;
        private readonly int _arrayPreview;
        private readonly int _maxStringChars;
        private uint _version;

        public GgufParser(string path, int arrayPreview, int maxStringChars)
        {
            _stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
                                     1 << 16, FileOptions.SequentialScan);
            _reader = new BinaryReader(_stream, Encoding.UTF8, false);
            _arrayPreview = arrayPreview;
            _maxStringChars = maxStringChars;
        }

        public void Dispose()
        {
            _reader.Dispose();
        }

        private long Remaining
        {
            get { return _stream.Length - _stream.Position; }
        }

        // GGUF v1 used 32-bit counts and lengths; v2 and v3 use 64-bit.
        private int CountSize
        {
            get { return _version == 1 ? 4 : 8; }
        }

        public GgufFile Parse()
        {
            if (_stream.Length < 16)
                throw new InvalidDataException("File is too small to be a GGUF file.");

            uint magic = _reader.ReadUInt32();
            if (magic == MagicBigEndian)
                throw new NotSupportedException("Big-endian GGUF files are not supported.");
            if (magic != MagicLittleEndian)
                throw new InvalidDataException(string.Format("Not a GGUF file (magic bytes 0x{0:X8}).", magic));

            _version = _reader.ReadUInt32();
            if (_version < 1 || _version > 3)
                throw new NotSupportedException(string.Format("Unsupported GGUF version {0}.", _version));

            ulong tensorCount = ReadCount();
            ulong metadataCount = ReadCount();
            if (tensorCount > MaxTensorCount)
                throw new InvalidDataException(string.Format("Tensor count {0} exceeds the limit of {1}.", tensorCount, MaxTensorCount));
            if (metadataCount > MaxMetadataCount)
                throw new InvalidDataException(string.Format("Metadata count {0} exceeds the limit of {1}.", metadataCount, MaxMetadataCount));

            var file = new GgufFile
            {
                FileSize = _stream.Length,
                Version = _version,
                TensorCount = tensorCount,
                MetadataCount = metadataCount
            };

            for (ulong i = 0; i < metadataCount; i++)
            {
                string key = ReadString();
                GgufValueType type = ReadValueType();
                string display;

                if (key == "general.alignment" && type == GgufValueType.UInt32)
                {
                    uint alignment = _reader.ReadUInt32();
                    if (alignment == 0 || (alignment & (alignment - 1)) != 0)
                        throw new InvalidDataException(string.Format("general.alignment must be a power of two (got {0}).", alignment));
                    file.Alignment = alignment;
                    display = alignment.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    display = ReadValueDisplay(type, 0);
                }

                file.Metadata.Add(new MetadataEntry(TextUtil.Sanitize(key, 200), TypeName(type), display));
            }

            for (ulong i = 0; i < tensorCount; i++)
                file.Tensors.Add(ReadTensorInfo());

            // Tensor data starts at the next multiple of the alignment after the header.
            ulong position = (ulong)_stream.Position;
            file.DataOffset = checked((position + file.Alignment - 1) / file.Alignment * file.Alignment);
            return file;
        }

        public static string GgmlTypeName(uint type)
        {
            string name;
            return GgmlTypeNames.TryGetValue(type, out name) ? name : "type_" + type.ToString(CultureInfo.InvariantCulture);
        }

        private TensorInfo ReadTensorInfo()
        {
            string name = TextUtil.Sanitize(ReadString(), 200);

            uint dimCount = _reader.ReadUInt32();
            if (dimCount > MaxDimensions)
                throw new InvalidDataException(string.Format("Tensor '{0}' has {1} dimensions (limit {2}).", name, dimCount, MaxDimensions));

            var dims = new ulong[dimCount];
            ulong elements = 1;
            for (int d = 0; d < dimCount; d++)
            {
                dims[d] = ReadCount();
                elements = checked(elements * dims[d]);
            }

            uint ggmlType = _reader.ReadUInt32();
            ulong offset = _reader.ReadUInt64();
            return new TensorInfo(name, dims, ggmlType, offset, elements);
        }

        private ulong ReadCount()
        {
            return _version == 1 ? _reader.ReadUInt32() : _reader.ReadUInt64();
        }

        private void EnsureAvailable(ulong bytes)
        {
            if (bytes > (ulong)Remaining)
                throw new InvalidDataException("A declared size runs past the end of the file (truncated or corrupt).");
        }

        private string ReadString()
        {
            ulong length = ReadCount();
            if (length > MaxStringBytes)
                throw new InvalidDataException(string.Format("String length {0} exceeds the limit of {1} bytes.", length, MaxStringBytes));
            EnsureAvailable(length);

            byte[] bytes = _reader.ReadBytes((int)length);
            if ((ulong)bytes.Length != length)
                throw new EndOfStreamException();
            return Encoding.UTF8.GetString(bytes); // invalid sequences become U+FFFD
        }

        private GgufValueType ReadValueType()
        {
            uint raw = _reader.ReadUInt32();
            if (!Enum.IsDefined(typeof(GgufValueType), raw))
                throw new InvalidDataException(string.Format("Unknown metadata value type {0}.", raw));
            return (GgufValueType)raw;
        }

        // Returns the encoded size of fixed-width types, or 0 for strings and arrays.
        private static int FixedSize(GgufValueType type)
        {
            switch (type)
            {
                case GgufValueType.UInt8:
                case GgufValueType.Int8:
                case GgufValueType.Bool:
                    return 1;
                case GgufValueType.UInt16:
                case GgufValueType.Int16:
                    return 2;
                case GgufValueType.UInt32:
                case GgufValueType.Int32:
                case GgufValueType.Float32:
                    return 4;
                case GgufValueType.UInt64:
                case GgufValueType.Int64:
                case GgufValueType.Float64:
                    return 8;
                default:
                    return 0;
            }
        }

        private string ReadValueDisplay(GgufValueType type, int depth)
        {
            CultureInfo inv = CultureInfo.InvariantCulture;
            switch (type)
            {
                case GgufValueType.UInt8: return _reader.ReadByte().ToString(inv);
                case GgufValueType.Int8: return _reader.ReadSByte().ToString(inv);
                case GgufValueType.UInt16: return _reader.ReadUInt16().ToString(inv);
                case GgufValueType.Int16: return _reader.ReadInt16().ToString(inv);
                case GgufValueType.UInt32: return _reader.ReadUInt32().ToString(inv);
                case GgufValueType.Int32: return _reader.ReadInt32().ToString(inv);
                case GgufValueType.UInt64: return _reader.ReadUInt64().ToString(inv);
                case GgufValueType.Int64: return _reader.ReadInt64().ToString(inv);
                case GgufValueType.Float32: return _reader.ReadSingle().ToString("G9", inv);
                case GgufValueType.Float64: return _reader.ReadDouble().ToString("G17", inv);
                case GgufValueType.Bool: return _reader.ReadByte() != 0 ? "true" : "false";
                case GgufValueType.String: return "\"" + TextUtil.Sanitize(ReadString(), _maxStringChars) + "\"";
                case GgufValueType.Array: return ReadArrayDisplay(depth);
                default:
                    throw new InvalidDataException(string.Format("Unknown metadata value type {0}.", (uint)type));
            }
        }

        private string ReadArrayDisplay(int depth)
        {
            if (depth >= MaxArrayDepth)
                throw new InvalidDataException("Metadata arrays are nested too deeply.");

            GgufValueType elementType = ReadValueType();
            ulong count = ReadCount();
            CheckArrayFits(elementType, count);

            int shown = (int)Math.Min(count, (ulong)_arrayPreview);
            var parts = new List<string>(shown);
            for (int i = 0; i < shown; i++)
                parts.Add(ReadValueDisplay(elementType, depth + 1));

            // Skip the rest without materialising it (e.g. 150k-entry vocabularies).
            SkipValues(elementType, count - (ulong)shown, depth + 1);

            string body = string.Join(", ", parts);
            if (count > (ulong)shown)
                body += shown > 0 ? ", ..." : "...";
            return string.Format("[{0}] ({1:N0} x {2})", body, count, TypeName(elementType));
        }

        private void CheckArrayFits(GgufValueType elementType, ulong count)
        {
            // Smallest possible encoding of one element; rejects absurd counts up front.
            int minSize = FixedSize(elementType);
            if (minSize == 0)
                minSize = elementType == GgufValueType.String ? CountSize : 4 + CountSize;

            if (count > (ulong)Remaining / (ulong)minSize)
                throw new InvalidDataException(string.Format("Array of {0} elements cannot fit in the remaining file.", count));
        }

        private void SkipValues(GgufValueType type, ulong count, int depth)
        {
            if (count == 0) return;

            int fixedSize = FixedSize(type);
            if (fixedSize > 0)
            {
                ulong bytes = checked(count * (ulong)fixedSize);
                EnsureAvailable(bytes);
                _stream.Seek((long)bytes, SeekOrigin.Current);
                return;
            }

            for (ulong i = 0; i < count; i++)
            {
                if (type == GgufValueType.String)
                {
                    ulong length = ReadCount();
                    EnsureAvailable(length);
                    _stream.Seek((long)length, SeekOrigin.Current);
                }
                else // nested array
                {
                    if (depth >= MaxArrayDepth)
                        throw new InvalidDataException("Metadata arrays are nested too deeply.");
                    GgufValueType innerType = ReadValueType();
                    ulong innerCount = ReadCount();
                    CheckArrayFits(innerType, innerCount);
                    SkipValues(innerType, innerCount, depth + 1);
                }
            }
        }

        private static string TypeName(GgufValueType type)
        {
            return type.ToString().ToLowerInvariant();
        }
    }

    internal static class TextUtil
    {
        // Escapes control characters and bidirectional-override characters so that
        // strings from the file cannot inject terminal escape sequences or visually
        // reorder output. Optionally truncates long values (maxChars <= 0 = no limit).
        public static string Sanitize(string value, int maxChars)
        {
            int limit = maxChars > 0 ? Math.Min(value.Length, maxChars) : value.Length;
            var sb = new StringBuilder(limit + 16);

            for (int i = 0; i < limit; i++)
            {
                char c = value[i];
                switch (c)
                {
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    default:
                        bool bidi = (c >= '\u202A' && c <= '\u202E') || (c >= '\u2066' && c <= '\u2069');
                        if (char.IsControl(c) || bidi)
                            sb.Append("\\u").Append(((int)c).ToString("X4", CultureInfo.InvariantCulture));
                        else
                            sb.Append(c);
                        break;
                }
            }

            if (limit < value.Length)
                sb.AppendFormat("... ({0:N0} chars, use --full to show all)", value.Length);
            return sb.ToString();
        }

        public static string FormatBytes(long bytes)
        {
            string[] units = { "B", "KiB", "MiB", "GiB", "TiB" };
            double size = bytes;
            int unit = 0;
            while (size >= 1024 && unit < units.Length - 1)
            {
                size /= 1024;
                unit++;
            }
            return string.Format("{0:0.##} {1}", size, units[unit]);
        }

        public static string FormatCount(ulong n)
        {
            if (n >= 1000000000) return string.Format("{0:0.##}B", n / 1e9);
            if (n >= 1000000) return string.Format("{0:0.##}M", n / 1e6);
            if (n >= 1000) return string.Format("{0:0.##}K", n / 1e3);
            return n.ToString(CultureInfo.InvariantCulture);
        }
    }

    internal static class Program
    {
        private static int Main(string[] args)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            Console.OutputEncoding = Encoding.UTF8;

            string path = null;
            bool showTensors = false;
            bool full = false;
            int preview = 5;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-t":
                    case "--tensors":
                        showTensors = true;
                        break;
                    case "-f":
                    case "--full":
                        full = true;
                        break;
                    case "-p":
                    case "--preview":
                        if (i + 1 >= args.Length || !int.TryParse(args[++i], out preview) || preview < 0 || preview > 1000)
                            return Fail("--preview needs a number between 0 and 1000.");
                        break;
                    case "-h":
                    case "--help":
                        PrintUsage();
                        return 0;
                    default:
                        if (args[i].StartsWith("-", StringComparison.Ordinal))
                            return Fail("Unknown option '" + TextUtil.Sanitize(args[i], 50) + "'.");
                        if (path != null)
                            return Fail("Only one file can be given.");
                        path = args[i];
                        break;
                }
            }

            if (path == null)
            {
                PrintUsage();
                return 1;
            }

            string fullPath;
            try
            {
                fullPath = Path.GetFullPath(path);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException || ex is PathTooLongException)
            {
                return Fail("The file path is not valid.");
            }

            if (!File.Exists(fullPath))
                return Fail("File not found.");

            try
            {
                using (var parser = new GgufParser(fullPath, preview, full ? 0 : 256))
                {
                    GgufFile file = parser.Parse();
                    Print(file, fullPath, showTensors);
                }
                return 0;
            }
            catch (EndOfStreamException) { return Fail("The file ended unexpectedly (truncated or not a GGUF file)."); }
            catch (InvalidDataException ex) { return Fail("Invalid GGUF file: " + ex.Message); }
            catch (NotSupportedException ex) { return Fail(ex.Message); }
            catch (OverflowException) { return Fail("Invalid GGUF file: a size or count overflowed."); }
            catch (UnauthorizedAccessException) { return Fail("Access to the file was denied."); }
            catch (IOException ex) { return Fail("Could not read the file: " + ex.Message); }
        }

        private static void Print(GgufFile f, string path, bool showTensors)
        {
            Console.WriteLine("File:            " + TextUtil.Sanitize(Path.GetFileName(path), 260));
            Console.WriteLine("File size:       {0} ({1:N0} bytes)", TextUtil.FormatBytes(f.FileSize), f.FileSize);
            Console.WriteLine("GGUF version:    {0}", f.Version);
            Console.WriteLine("Tensor count:    {0:N0}", f.TensorCount);
            Console.WriteLine("Metadata count:  {0:N0}", f.MetadataCount);
            Console.WriteLine("Alignment:       {0}", f.Alignment);
            Console.WriteLine("Data offset:     0x{0:X} ({1:N0} bytes of header)", f.DataOffset, f.DataOffset);
            Console.WriteLine();

            Console.WriteLine("Metadata");
            Console.WriteLine("--------");
            int keyWidth = f.Metadata.Count == 0 ? 0 : Math.Min(48, f.Metadata.Max(m => m.Key.Length));
            foreach (MetadataEntry m in f.Metadata)
                Console.WriteLine("  {0}  {1,-10} {2}", m.Key.PadRight(keyWidth), "[" + m.TypeName + "]", m.Display);
            Console.WriteLine();

            ulong totalParams = 0;
            int outOfRange = 0;
            foreach (TensorInfo t in f.Tensors)
            {
                totalParams = checked(totalParams + t.ElementCount);
                if (t.Offset > (ulong)f.FileSize || f.DataOffset > (ulong)f.FileSize - t.Offset)
                    outOfRange++;
            }

            Console.WriteLine("Tensors");
            Console.WriteLine("-------");
            Console.WriteLine("  Total parameters: {0} ({1:N0})", TextUtil.FormatCount(totalParams), totalParams);
            foreach (IGrouping<uint, TensorInfo> g in f.Tensors.GroupBy(t => t.GgmlType).OrderByDescending(g => g.Count()))
            {
                ulong groupParams = g.Aggregate(0UL, (sum, t) => checked(sum + t.ElementCount));
                Console.WriteLine("  {0,-8} {1,6:N0} tensors  {2,10} params",
                    GgufParser.GgmlTypeName(g.Key), g.Count(), TextUtil.FormatCount(groupParams));
            }
            if (outOfRange > 0)
                Console.WriteLine("  WARNING: {0} tensor(s) point past the end of the file.", outOfRange);

            if (!showTensors)
            {
                Console.WriteLine();
                Console.WriteLine("  (use --tensors to list every tensor)");
                return;
            }

            Console.WriteLine();
            int nameWidth = f.Tensors.Count == 0 ? 4 : Math.Max(4, Math.Min(60, f.Tensors.Max(t => t.Name.Length)));
            Console.WriteLine("  {0,5}  {1}  {2,-8}  {3,-24}  {4,15}  Offset", "#", "Name".PadRight(nameWidth), "Type", "Shape", "Elements");
            for (int i = 0; i < f.Tensors.Count; i++)
            {
                TensorInfo t = f.Tensors[i];
                string shape = "[" + string.Join(", ", t.Dims) + "]";
                Console.WriteLine("  {0,5}  {1}  {2,-8}  {3,-24}  {4,15:N0}  0x{5:X}",
                    i, t.Name.PadRight(nameWidth), GgufParser.GgmlTypeName(t.GgmlType), shape, t.ElementCount, t.Offset);
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("gguf-info - display the header of a GGUF model file");
            Console.WriteLine();
            Console.WriteLine("Usage: gguf-info <model.gguf> [options]");
            Console.WriteLine("  -t, --tensors     list every tensor (name, type, shape, offset)");
            Console.WriteLine("  -f, --full        show long strings (e.g. chat templates) in full");
            Console.WriteLine("  -p, --preview N   array elements to preview (default 5, max 1000)");
            Console.WriteLine("  -h, --help        show this help");
        }

        private static int Fail(string message)
        {
            Console.Error.WriteLine("Error: " + message);
            return 1;
        }
    }
}