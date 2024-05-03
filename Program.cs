using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace DbfDeserializer
{
    public enum BOOL : byte { False = 0, True = 1 }
    public enum FoxProFileType : byte
    {
        FoxBase_Low = 0x02,
        FoxBasePlus_NoMemo = 0x03,
        VisualFoxPro_NoAutoInc = 0x30,
        VisualFoxPro_AutoInc = 0x31,
        VisualFoxPro_Var = 0x32,
        Dbase4_SqlTables_NoMemo = 0x43,
        Dbase4_SqlSys_NoMemo = 0x63,
        FoxBasePlus_Memo = 0x83,
        Dbase4_Memo = 0x8B,
        Dbase4_SqlTables_Memo = 0xCB,
        FoxPro_2x_Memo = 0xF5,
        HiPerSix_SmtMemo = 0xE5,
        FoxBase_High = 0xFB
    }
    [Flags] public enum TableFlags : byte
    {
        HasCdx = 0x01,
        HasMemo = 0x02,
        HasDbc = 0x04,

        HasMemoAndCdx = HasCdx + HasMemo,
        HasCdxAndDbc = HasCdx + HasDbc,
        HasMemoAndDbc = HasMemo + HasDbc,

        HasCdxAndMemoAndDbc = HasCdx + HasMemo + HasDbc
    }
    [Flags] public enum FieldFlags : byte
    {
        SysColumn = 0x01,
        Nullable = 0x02,
        CharOrMemo = 0x04,
        NullableBinary = 0x06,
        AutoIncrementing = 0x0C
    }
    public enum CodepageId : byte
    {
        NULL = 0x00,

        DOS_USA = 0x01,
        DOS_International = 0x02,
        Windows_ANSI = 0x03,
        OSX = 0x04,

        DOS_EasternEuropean = 0x64,
        DOS_Russian = 0x65,
        DOS_Nordic = 0x66,
        DOS_Icelandic = 0x67,
        DOS_Czech = 0x68,
        DOS_Polish = 0x69,
        DOS_Greek = 0x6A,
        DOS_Turkish = 0x6B,

        Windows_ChineseTraditional = 0x78,
        Windows_Korean = 0x79,
        Windows_ChineseSimple = 0x7A,
        Windows_Japanese = 0x7B,
        Windows_Thai = 0x7C,
        Windows_Hebrew = 0x7D,
        Windows_Arabic = 0x7E,

        Windows_EasternEuropean = 0xC8,
        Windows_Russian = 0xC9,
        Windows_Turkish = 0xCA,
        Windows_Greek = 0xCB,

        OSX_RUS = 0x96,
        OSX_EE = 0x97,
        OSX_GREEK = 0x98
    }
    public enum FieldType : byte
    {
        Char = (byte)'C',
        Date = (byte)'D',
        Float = (byte)'F',
        Logic = (byte)'L',
        Memo = (byte)'M',
        Num = (byte)'N',
        Blob = (byte)'W',
        Currency = (byte)'Y',
        Double = (byte)'B',
        DateAndTime = (byte)'T',
        General = (byte)'G',
        Integer = (byte)'I',
        Varbinary = (byte)'Q',
        Varchar = (byte)'V'
    }
    public enum BlockSignature : int { Image = 0, Text = 1, Object = 2 }
    public enum AccessQualifier { NULL = 0, Public, Private, Protected, Internal }
    public enum Modifier { NULL = 0, Static, Readonly, Constant, StaticReadonly }

    #region Union data structures

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Size = 10, Pack = 1)]
    public struct RESERVE_10
    {
        public byte r0; public byte r1;
        public byte r2; public byte r3;
        public byte r4; public byte r5;
        public byte r6; public byte r7;
        public byte r8; public byte r9;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 4, CharSet = CharSet.Ansi)]
    public struct i32
    {
        [FieldOffset(0)]
        public int i32_full;

        [FieldOffset(0)]
        public uint u32_full;

        [FieldOffset(0)]
        public float f32_full;

        [FieldOffset(0)]
        public short i16_top;

        [FieldOffset(2)]
        public short i16_bot;

        [FieldOffset(0)]
        public short u16_top;

        [FieldOffset(2)]
        public short u16_bot;

        [FieldOffset(0)]
        public byte b0;

        [FieldOffset(1)]
        public byte b1;

        [FieldOffset(2)]
        public byte b2;

        [FieldOffset(3)]
        public byte b3;

        public i32(byte _b0, byte _b1, byte _b2, byte _b3)
        {
            i32_full = 0;
            u32_full = 0;
            f32_full = 0f;
            i16_top = 0;
            i16_bot = 0;
            u16_top = 0;
            u16_bot = 0;
            b0 = _b0;
            b1 = _b1;
            b2 = _b2;
            b3 = _b3;
        }

        public static int b2i(byte _b0, byte _b1, byte _b2, byte _b3)
        {
            return new i32(_b0, _b1, _b2, _b3).i32_full;
        }

        public static uint b2u(byte _b0, byte _b1, byte _b2, byte _b3)
        {
            return new i32(_b0, _b1, _b2, _b3).u32_full;
        }
    }

    [StructLayout(LayoutKind.Explicit, Pack = 2, CharSet = CharSet.Ansi)]
    public struct i16
    {
        [FieldOffset(0)]
        public short full;

        [FieldOffset(0)]
        public ushort ufull;

        [FieldOffset(0)]
        public byte b0;

        [FieldOffset(1)]
        public byte b1;

        public i16(byte _b0, byte _b1)
        {
            full = 0;
            ufull = 0;

            b0 = _b0;
            b1 = _b1;
        }

        public static short b2i(byte _b0, byte _b1)
        {
            return new i16(_b0, _b1).full;
        }

        public static ushort b2u(byte _b0, byte _b1)
        {
            return new i16(_b0, _b1).ufull;
        }
    }

    [StructLayout(LayoutKind.Explicit, Pack = 8, CharSet = CharSet.Ansi)]
    public struct i64
    {
        [FieldOffset(0)]
        public long i64_full;

        [FieldOffset(0)]
        public ulong u64_full;

        [FieldOffset(0)]
        public i32 i32_top;

        [FieldOffset(1)]
        public i32 i32_bot;

        public i64(long _v)
        {
            i32_top = new();
            i32_bot = new();
            u64_full = 0;
            i64_full = _v;
        }
    }

    #endregion

    public static unsafe class Helper
    {
        public static readonly char[] TRIM_PATH = new char[] { '\\', '/' };

        /// <summary>
        ///
        /// Hash based on djb2 algorithm.
        ///
        /// <para>
        /// WARNING(RYAN_2023-09-22): DO NOT USE THIS FOR CRYPTOGRAPHIC PURPOSES!
        ///
        /// WARNING(RYAN_2023-12-18):   DO NOT MODIFY WITHOUT CHANGING
        ///                             CONSTANT HASH VALUES!
        /// </para>
        ///
        /// </summary>
        ///
        /// <param name="str">String to convert to hash.</param>
        ///
        /// <returns>Unsigned 64 bit hash integer.</returns>
        public static ulong HashUL(in string str)
        {
            int len = str.Length;
            ulong hash = 5381ul;
            fixed (char* fxd_str = str)
            {
                char* c = fxd_str;
                while (--len >= 0)
                {
                    hash = ((hash << 5) + hash) + *c++; // hash * 33 + c
                }
            }

            return hash;
        }

        /// <summary>
        ///
        /// Determines whether or not an absolute input path is an invalid
        /// Windows file path.
        ///
        /// </summary>
        ///
        /// <param name="p">Input path to check.</param>
        ///
        /// <returns>
        ///
        /// <br>True if the input path is INVALID.</br>
        /// <br>False if the input path is VALID.</br>
        ///
        /// </returns>
        public static bool InvalidAbsPath(string p)
        {
            const int MAX_PATH_LEN = 255;
            if (p == null || p.Length < 3 || p.Length > MAX_PATH_LEN)
                return true;

            try
            {
                string fullPath = Path.GetFullPath(p);
                string? root = Path.GetPathRoot(p);
                if (string.IsNullOrEmpty(root)) { return false; }
                else
                {
                    return string.IsNullOrEmpty(root.Trim(TRIM_PATH));
                }
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        ///
        /// Determines whether or not an input directory path is invalid.
        ///
        /// <para>
        /// NOTE(RYAN_2024-02-13):  Absolute path is REQUIRED.
        ///                         Does not support relative path addressing.
        /// </para>
        ///
        /// </summary>
        ///
        /// <param name="p">Input directory path to check.</param>
        ///
        /// <returns>
        ///
        /// <br>True if the input path is INVALID.</br>
        /// <br>False if the input path is VALID.</br>
        ///
        /// </returns>
        public static bool InvalidDir(string p)
        {
            if (InvalidAbsPath(p))
                return true;

            try
            {
                FileAttributes fa = File.GetAttributes(p);
                return (fa & FileAttributes.Directory) != FileAttributes.Directory ||
                        Directory.Exists(p) == false;
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        ///
        /// Determines whether or not an input FILE path is invalid.
        ///
        /// <para>
        /// NOTE(RYAN_2024-02-13):  Absolute path is REQUIRED.
        ///                         Does not support relative path addressing.
        /// </para>
        ///
        /// </summary>
        ///
        /// <param name="p">Input file path to check.</param>
        ///
        /// <returns>
        ///
        /// <br>True if the input FILE path is INVALID.</br>
        /// <br>False if the input FILE path is VALID.</br>
        ///
        /// </returns>
        public static bool InvalidFileDir(string p)
        {
#pragma warning disable CS8604 // Possible null reference argument.

            //NOTE(RYAN_2024-02-13):InvalidDir() will handle instances of a
            //                      null string reference.
            return InvalidDir(Path.GetDirectoryName(p));

#pragma warning restore CS8604 // Possible null reference argument.
        }
    }

    public static class CsGenUtl
    {
        public const ulong _Boolean = 229419558205541ul;
        public const ulong _Byte = 6383948441ul;
        public const ulong _SByte = 210688488716ul;
        public const ulong _Char = 6383965251ul;
        public const ulong _Decimal = 229421735457396ul;
        public const ulong _Double = 6952186300160ul;
        public const ulong _Single = 6952765969415ul;
        public const ulong _Int32 = 210678203093ul;
        public const ulong _UInt32 = 6952806302954ul;
        public const ulong _IntPtr = 6952380735942ul;
        public const ulong _UIntPtr = 229442608031355ul;
        public const ulong _Int64 = 210678203194ul;
        public const ulong _UInt64 = 6952806303055ul;
        public const ulong _Int16 = 210678203031ul;
        public const ulong _UInt16 = 6952806302892ul;
        public const ulong _Object = 6952600980188ul;
        public const ulong _String = 6952779160540ul;
        public const ulong _DateTime = 7570912763890610ul;

        public static readonly string _String_STR = "string";
        public static readonly string _Char_STR = "char";
        public static readonly string _DateTime_STR = "DateTime";
        public static readonly string _Boolean_STR = "bool";

        public static readonly string _Byte_STR = "byte";
        public static readonly string _SByte_STR = "sbyte";
        public static readonly string _Decimal_STR = "decimal";
        public static readonly string _Double_STR = "double";
        public static readonly string _Single_STR = "float";
        public static readonly string _Int32_STR = "int";
        public static readonly string _UInt32_STR = "uint";
        public static readonly string _IntPtr_STR = "IntPtr";
        public static readonly string _UIntPtr_STR = "UIntPtr";
        public static readonly string _Int64_STR = "long";
        public static readonly string _UInt64_STR = "ulong";
        public static readonly string _Int16_STR = "short";
        public static readonly string _UInt16_STR = "ushort";
        public static readonly string _Object_STR = "object";

        public static readonly string _DEFAULT_INIT_String_STR = "= string.Empty;";
        public static readonly string _DEFAULT_INIT_DateTime_STR = "= DateTime.MinValue;";

        public static readonly string _DEFAULT_INIT_Byte_STR = "= 0;";
        public static readonly string _DEFAULT_INIT_SByte_STR = "= 0;";
        public static readonly string _DEFAULT_INIT_Int16_STR = "= 0;";
        public static readonly string _DEFAULT_INIT_UInt16_STR = "= 0;";
        public static readonly string _DEFAULT_INIT_Int32_STR = "= 0;";
        public static readonly string _DEFAULT_INIT_UInt32_STR = "= 0U;";
        public static readonly string _DEFAULT_INIT_Int64_STR = "= 0L;";
        public static readonly string _DEFAULT_INIT_UInt64_STR = "= 0UL;";

        public static readonly string _DEFAULT_INIT_Decimal_STR = "= 0.0M;";
        public static readonly string _DEFAULT_INIT_Double_STR = "= 0.0D;";
        public static readonly string _DEFAULT_INIT_Single_STR = "= 0.0F;";

        public static readonly string _DEFAULT_INIT_Object_STR = "= new();";

        public static readonly string _DEFAULT_INIT_IntPtr_STR = "= IntPtr.Zero;";
        public static readonly string _DEFAULT_INIT_UIntPtr_STR = "= UIntPtr.Zero;";
    }

    public class CsField
    {
        public AccessQualifier qualifier;
        public Modifier modifier;

        public string FieldName;

        public Type FieldType;
        public readonly ulong TypeNameId;

        public string defaultInit;

        public string GetDefaultInit()
        {
            switch (TypeNameId)
            {
                case CsGenUtl._String: return CsGenUtl._DEFAULT_INIT_String_STR;
                case CsGenUtl._Char: return string.Empty;

                case CsGenUtl._Boolean: return string.Empty;
                case CsGenUtl._DateTime: return CsGenUtl._DEFAULT_INIT_DateTime_STR;

                case CsGenUtl._Decimal: return CsGenUtl._DEFAULT_INIT_Decimal_STR;
                case CsGenUtl._Double: return CsGenUtl._DEFAULT_INIT_Double_STR;
                case CsGenUtl._Single: return CsGenUtl._DEFAULT_INIT_Single_STR;

                case CsGenUtl._Byte: return CsGenUtl._DEFAULT_INIT_Byte_STR;
                case CsGenUtl._SByte: return CsGenUtl._DEFAULT_INIT_SByte_STR;
                case CsGenUtl._Int16: return CsGenUtl._DEFAULT_INIT_Int16_STR;
                case CsGenUtl._UInt16: return CsGenUtl._DEFAULT_INIT_UInt16_STR;
                case CsGenUtl._Int32: return CsGenUtl._DEFAULT_INIT_Int32_STR;
                case CsGenUtl._UInt32: return CsGenUtl._DEFAULT_INIT_UInt32_STR;
                case CsGenUtl._Int64: return CsGenUtl._DEFAULT_INIT_Int64_STR;
                case CsGenUtl._UInt64: return CsGenUtl._DEFAULT_INIT_UInt64_STR;

                case CsGenUtl._IntPtr: return CsGenUtl._DEFAULT_INIT_IntPtr_STR;
                case CsGenUtl._UIntPtr: return CsGenUtl._DEFAULT_INIT_UIntPtr_STR;

                default: return CsGenUtl._DEFAULT_INIT_Object_STR;
            }
        }

        public string GetTypename()
        {
            switch (TypeNameId)
            {
                case CsGenUtl._String: return CsGenUtl._String_STR;
                case CsGenUtl._Char: return CsGenUtl._Char_STR;

                case CsGenUtl._DateTime: return CsGenUtl._DateTime_STR;
                case CsGenUtl._Boolean: return CsGenUtl._Boolean_STR;

                case CsGenUtl._Single: return CsGenUtl._Single_STR;
                case CsGenUtl._Double: return CsGenUtl._Double_STR;
                case CsGenUtl._Decimal: return CsGenUtl._Decimal_STR;

                case CsGenUtl._Byte: return CsGenUtl._Byte_STR;
                case CsGenUtl._SByte: return CsGenUtl._SByte_STR;
                case CsGenUtl._Int16: return CsGenUtl._Int16_STR;
                case CsGenUtl._UInt16: return CsGenUtl._UInt16_STR;
                case CsGenUtl._Int32: return CsGenUtl._Int32_STR;
                case CsGenUtl._UInt32: return CsGenUtl._UInt32_STR;
                case CsGenUtl._Int64: return CsGenUtl._Int64_STR;
                case CsGenUtl._UInt64: return CsGenUtl._UInt64_STR;

                case CsGenUtl._IntPtr: return CsGenUtl._IntPtr_STR;
                case CsGenUtl._UIntPtr: return CsGenUtl._UIntPtr_STR;

                default: return CsGenUtl._Object_STR;
            }
        }

        public CsField(AccessQualifier aq, Modifier mod, string name, Type t)
        {
            qualifier = aq;
            modifier = mod;
            FieldName = name;

            FieldType = t;
            TypeNameId = Helper.HashUL(t.Name);
            defaultInit = GetDefaultInit();
        }
    }

    public class CsProperty
    {
        public AccessQualifier qualifier;
        public Modifier modifier;

        public string FieldName;

        public Type FieldType;
        public readonly ulong TypeNameId;

        public string defaultInit;

        public string GetDefaultInit()
        {
            switch (TypeNameId)
            {
                case CsGenUtl._String: return CsGenUtl._DEFAULT_INIT_String_STR;
                case CsGenUtl._Char: return string.Empty;

                case CsGenUtl._Boolean: return string.Empty;
                case CsGenUtl._DateTime: return CsGenUtl._DEFAULT_INIT_DateTime_STR;

                case CsGenUtl._Decimal: return CsGenUtl._DEFAULT_INIT_Decimal_STR;
                case CsGenUtl._Double: return CsGenUtl._DEFAULT_INIT_Double_STR;
                case CsGenUtl._Single: return CsGenUtl._DEFAULT_INIT_Single_STR;

                case CsGenUtl._Byte: return CsGenUtl._DEFAULT_INIT_Byte_STR;
                case CsGenUtl._SByte: return CsGenUtl._DEFAULT_INIT_SByte_STR;
                case CsGenUtl._Int16: return CsGenUtl._DEFAULT_INIT_Int16_STR;
                case CsGenUtl._UInt16: return CsGenUtl._DEFAULT_INIT_UInt16_STR;
                case CsGenUtl._Int32: return CsGenUtl._DEFAULT_INIT_Int32_STR;
                case CsGenUtl._UInt32: return CsGenUtl._DEFAULT_INIT_UInt32_STR;
                case CsGenUtl._Int64: return CsGenUtl._DEFAULT_INIT_Int64_STR;
                case CsGenUtl._UInt64: return CsGenUtl._DEFAULT_INIT_UInt64_STR;

                case CsGenUtl._IntPtr: return CsGenUtl._DEFAULT_INIT_IntPtr_STR;
                case CsGenUtl._UIntPtr: return CsGenUtl._DEFAULT_INIT_UIntPtr_STR;

                default: return CsGenUtl._DEFAULT_INIT_Object_STR;
            }
        }

        public string GetTypename()
        {
            switch (TypeNameId)
            {
                case CsGenUtl._String: return CsGenUtl._String_STR;
                case CsGenUtl._Char: return CsGenUtl._Char_STR;

                case CsGenUtl._DateTime: return CsGenUtl._DateTime_STR;
                case CsGenUtl._Boolean: return CsGenUtl._Boolean_STR;

                case CsGenUtl._Single: return CsGenUtl._Single_STR;
                case CsGenUtl._Double: return CsGenUtl._Double_STR;
                case CsGenUtl._Decimal: return CsGenUtl._Decimal_STR;

                case CsGenUtl._Byte: return CsGenUtl._Byte_STR;
                case CsGenUtl._SByte: return CsGenUtl._SByte_STR;
                case CsGenUtl._Int16: return CsGenUtl._Int16_STR;
                case CsGenUtl._UInt16: return CsGenUtl._UInt16_STR;
                case CsGenUtl._Int32: return CsGenUtl._Int32_STR;
                case CsGenUtl._UInt32: return CsGenUtl._UInt32_STR;
                case CsGenUtl._Int64: return CsGenUtl._Int64_STR;
                case CsGenUtl._UInt64: return CsGenUtl._UInt64_STR;

                case CsGenUtl._IntPtr: return CsGenUtl._IntPtr_STR;
                case CsGenUtl._UIntPtr: return CsGenUtl._UIntPtr_STR;

                default: return CsGenUtl._Object_STR;
            }
        }

        public CsProperty(AccessQualifier aq, Modifier mod, string name, Type t)
        {
            qualifier = aq;
            modifier = mod;
            FieldName = name;

            FieldType = t;
            TypeNameId = Helper.HashUL(t.Name);
            defaultInit = GetDefaultInit();
        }
    }

    public static class CsGen
    {
        public static readonly string[] QUALIFIERS = new string[5]
        {
            string.Empty, "public", "private", "protected", "internal"
        };
        public static readonly string[] MODIFIERS = new string[5]
        {
            string.Empty, "static", "readonly", "const", "static readonly"
        };
        public static readonly string FIELD_FMT = "\t{0} {1} {2} {3} {4}\r\n";
        public static readonly string FIELD_NO_MOD_FMT = "\t{0} {1} {2} {3} {4}\r\n";

        public static readonly string PROP_FMT = "\t{0} {1} {2} {3} {{ get; set; }} {4}\r\n";
        public static readonly string PROP_NO_MOD_FMT = "\t{0} {1} {2} {{ get; set; }} {3}\r\n";

        public static readonly string CLASS_DEF = "using System;\r\n\r\npublic class {0}\r\n{{\r\n{1}}}";

        public static string ConstructClass(string className, CsField[]? fields, CsProperty[]? properties)
        {
            if (string.IsNullOrWhiteSpace(className))
            {
                return string.Empty;
            }

            StringBuilder csDef = new();
            if (fields != null && fields.Length > -1)
            {
                CsField f;
                for (int i = 0, len = fields.Length; i < len; ++i)
                {
                    f = fields[i];

                    if (MODIFIERS[(int)f.modifier] == string.Empty)
                    {
                        csDef.AppendFormat(FIELD_NO_MOD_FMT,
                                            QUALIFIERS[(int)f.qualifier],
                                            f.GetTypename(),
                                            f.FieldName.ToLower(),
                                            f.defaultInit);
                    }
                    else
                    {
                        csDef.AppendFormat(FIELD_FMT,
                                            QUALIFIERS[(int)f.qualifier],
                                            MODIFIERS[(int)f.modifier],
                                            f.GetTypename(),
                                            f.FieldName.ToLower(),
                                            f.defaultInit);
                    }
                }
            }
            if (properties != null && properties.Length > -1)
            {
                CsProperty prop;
                for (int i = 0, len = properties.Length; i < len; ++i)
                {
                    prop = properties[i];

                    if (MODIFIERS[(int)prop.modifier] == string.Empty)
                    {
                        csDef.AppendFormat(PROP_NO_MOD_FMT,
                                            QUALIFIERS[(int)prop.qualifier],
                                            prop.GetTypename(),
                                            prop.FieldName.ToLower(),
                                            prop.defaultInit);
                    }
                    else
                    {
                        csDef.AppendFormat(PROP_FMT,
                                            QUALIFIERS[(int)prop.qualifier],
                                            MODIFIERS[(int)prop.modifier],
                                            prop.GetTypename(),
                                            prop.FieldName.ToLower(),
                                            prop.defaultInit);
                    }
                }
            }

            return string.Format(CLASS_DEF, className, csDef.ToString());
        }
    }

    public unsafe class RawFPTData
    {
        const int MEMO_BLOCK_HEADER_BYTE_SZ = 8;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct memo_hdr
        {
            public int freeBlockOffset;
            public ushort bytesPerBlock;
            public int endOffset;

            public memo_hdr(byte* data)
            {
                // NOTE(RYAN_2023-10-25):   FPT encodes this data in big endian.
                //                          Need to swap the bytes as a
                //                          consequence.
                freeBlockOffset = i32.b2i(data[3], data[2], data[1], data[0]);
                bytesPerBlock = i16.b2u(data[7], data[6]);

                endOffset = freeBlockOffset * bytesPerBlock;
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Ansi)]
        public struct memo_block_hdr
        {
            public BlockSignature DataType;
            public int blockOffset;
            public int memoByteLen;
            public int blockCt;
            public int blockStride;

            public memo_block_hdr(byte* data, int blockSzBtyeLen, int offset)
            {
                blockOffset = offset;

                // NOTE(RYAN_2023-10-25):   FPT encodes this data in big endian.
                //                          Need to swap the bytes as a
                //                          consequence.
                DataType = (BlockSignature)i32.b2i(data[3], data[2], data[1], data[0]);
                memoByteLen = i32.b2i(data[7], data[6], data[5], data[4]);
                blockCt = 1;
                // NOTE(RYAN_2024-03-14):   Take note that the header is
                //                          INCLUDED in the block size. So every
                //                          block is:
                //
                //                          { blockSz - header_size (8 bytes) }
                //
                //
                int leftoverBytes = blockSzBtyeLen - MEMO_BLOCK_HEADER_BYTE_SZ;
                if (memoByteLen > leftoverBytes)
                {
                    int len = memoByteLen - leftoverBytes;
                    do
                    {
                        ++blockCt;
                        len -= blockSzBtyeLen;
                    } while (len > 0);
                }
                blockStride = blockSzBtyeLen * blockCt;
            }
        }

        public class Memo
        {
            public memo_block_hdr header;
            public string content = string.Empty;

            public Memo(byte* blockStart, int memoBlockLen, int offset)
            {
                header = new(blockStart, memoBlockLen, offset);

                byte* memoStart = blockStart + MEMO_BLOCK_HEADER_BYTE_SZ;

                int len = header.memoByteLen;
                char* ascii = stackalloc char[len + 1];
                for (int i = 0; i < len; ++i)
                {
                    ascii[i] = (char)memoStart[i];
                }

                content = new string(ascii);
                content = content.Trim();
            }


        }

        public string path;
        public string filename;

        public memo_hdr memoHead;
        public byte[] data = Array.Empty<byte>();
        public Dictionary<int, Memo> Memos = new();

        public string GetMemoContent(int id)
        {
            int offset = id * memoHead.bytesPerBlock;
            if (Memos.TryGetValue(offset, out Memo? res))
                return res.content;

            return string.Empty;
        }

        public void GetMemos(byte* fxd_src)
        {
            const int MEMO_FILE_HEADER_BYTE_SZ = 512;
            byte* src = fxd_src + MEMO_FILE_HEADER_BYTE_SZ;

            Memo currMemo;
            byte* end = fxd_src + memoHead.endOffset;
            int offset;
            for (int i = 0; src < end; ++i)
            {
                offset = (int)(src - fxd_src);
                currMemo = new(src, memoHead.bytesPerBlock, offset);
                Memos.Add(offset, currMemo);
                src += currMemo.header.blockStride;
            }
        }

        public RawFPTData(string fptPath)
        {
            if (Helper.InvalidFileDir(fptPath))
                throw new ArgumentException($"Invalid FPT path ({fptPath}) passed to {nameof(RawFPTData)} constructor");

            FileInfo fi = new(fptPath);
            path = fi.FullName;
            filename = fi.Name;

            data = File.ReadAllBytes(fptPath);

            fixed (byte* fxd_data = data)
            {
                memoHead = new(fxd_data);
                GetMemos(fxd_data);
            }
        }
    }

    public unsafe class RawDbfData
    {
        #region DBF Helper Data Structures

        public class RawRecord
        {
            public bool deleted;
            public string[] content;

            public RawRecord()
            {
                deleted = false;
                content = Array.Empty<string>();
            }

            public RawRecord(bool recordDeleted, int columnCount)
            {
                deleted = recordDeleted;
                content = new string[columnCount];
            }
        }

        public class ColumnHeader
        {
            public string name;
            public FieldType dbfType;
            public Type csType;
            public int fieldRecPos; // Displacement of field in record
            public byte fieldLen;
            public byte fieldDecimalCt;
            public FieldFlags flags;
            public int nextIncVal;
            public byte nextStepVal;

            public static Type Field2Type(FieldType ft)
            {
                switch (ft)
                {
                    case FieldType.Char: return typeof(string);

                    case FieldType.DateAndTime:
                    case FieldType.Date: return typeof(DateTime);

                    case FieldType.Float: return typeof(float);
                    case FieldType.Logic: return typeof(bool);
                    case FieldType.Memo: return typeof(uint);
                    case FieldType.Currency: return typeof(decimal);

                    case FieldType.Double:
                    case FieldType.Num: return typeof(double);

                    case FieldType.Integer: return typeof(int);

                    default: return typeof(string);
                }
            }

            public ColumnHeader(field_head hdr)
            {
                name = hdr.name.ToString();
                dbfType = hdr.dbfType;
                csType = Field2Type(dbfType);
                fieldRecPos = hdr.fieldRecPos;
                fieldLen = hdr.fieldLen;
                fieldDecimalCt = hdr.fieldDecimalCt;
                flags = hdr.flags;
                nextIncVal = hdr.nextIncVal;
                nextStepVal = hdr.nextStepVal;
            }

            public ColumnHeader()
            {
                name = string.Empty;
                dbfType = FieldType.General;
                csType = Field2Type(dbfType);

                fieldRecPos = -1;
                fieldLen = 0;
                fieldDecimalCt = 0;
                flags = 0;
                nextIncVal = -1;
                nextStepVal = 0;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Size = 10, Pack = 1)]
        public struct field_name
        {
            public byte c0; public byte c1;
            public byte c2; public byte c3;
            public byte c4; public byte c5;
            public byte c6; public byte c7;
            public byte c8; public byte c9;
            public byte c10;

            public override readonly unsafe string ToString()
            {
                char* _name = stackalloc char[11] { (char)c0,
                                                    (char)c1,
                                                    (char)c2,
                                                    (char)c3,
                                                    (char)c4,
                                                    (char)c5,
                                                    (char)c6,
                                                    (char)c7,
                                                    (char)c8,
                                                    (char)c9,
                                                    (char)c10 };
                return new string(_name);
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Size = 32, Pack = 1)]
        public struct field_head
        {
            public field_name name;
            public FieldType dbfType;
            public int fieldRecPos; // Displacement of field in record
            public byte fieldLen;
            public byte fieldDecimalCt;
            public FieldFlags flags;
            public int nextIncVal;
            public byte nextStepVal;
            public long reserved_8;
        }

        [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
        public struct dos_mu_env
        {
            [FieldOffset(0)]
            public byte DOS_MU_ENV_0;

            [FieldOffset(1)]
            public byte DOS_MU_ENV_1;

            [FieldOffset(2)]
            public byte DOS_MU_ENV_2;

            [FieldOffset(3)]
            public byte DOS_MU_ENV_3;

            [FieldOffset(4)]
            public byte DOS_MU_ENV_4;

            [FieldOffset(5)]
            public byte DOS_MU_ENV_5;

            [FieldOffset(6)]
            public byte DOS_MU_ENV_6;

            [FieldOffset(7)]
            public byte DOS_MU_ENV_7;

            [FieldOffset(8)]
            public byte DOS_MU_ENV_8;

            [FieldOffset(9)]
            public byte DOS_MU_ENV_9;

            [FieldOffset(10)]
            public byte DOS_MU_ENV_10;

            [FieldOffset(11)]
            public byte DOS_MU_ENV_11;

            public dos_mu_env(byte _b0, byte _b1, byte _b2, byte _b3, byte _b4,
                                byte _b5, byte _b6, byte _b7, byte _b8, byte _b9,
                                byte _b10, byte _b11)
            {
                DOS_MU_ENV_0 = _b0;
                DOS_MU_ENV_1 = _b1;
                DOS_MU_ENV_2 = _b2;
                DOS_MU_ENV_3 = _b3;
                DOS_MU_ENV_4 = _b4;
                DOS_MU_ENV_5 = _b5;
                DOS_MU_ENV_6 = _b6;
                DOS_MU_ENV_7 = _b7;
                DOS_MU_ENV_8 = _b8;
                DOS_MU_ENV_9 = _b9;
                DOS_MU_ENV_10 = _b10;
                DOS_MU_ENV_11 = _b11;
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi, Size = 3)]
        public struct dbf_date
        {
            [MarshalAs(UnmanagedType.I1)]
            public byte year;
            [MarshalAs(UnmanagedType.I1)]
            public byte month;
            [MarshalAs(UnmanagedType.I1)]
            public byte day;

            public dbf_date(byte _year, byte _month, byte _day)
            {
                year = _year;
                month = _month;
                day = _day;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Size = 32)]
        public struct dbf_head
        {
            public byte dBASE_Info;

            public byte year;
            public byte month;
            public byte day;

            public int recordCt;
            public short hdrLen;
            public short recordStride;

            private readonly short RESERVED_0;

            public BOOL incompleteTransaction;
            public BOOL encrypted;
            public dos_mu_env MultiuserEnv;
            public byte tableFlags;
            public byte codepageId;

            private readonly short RESERVED_EOF;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct dbf_header
        {
            public FoxProFileType VfpFileType;
            public DateTime lastWrite;
            public int recordCt;

            public short hdrLen;
            public short recordStride; // size of record in bytes

            public bool incompleteTransaction;
            public bool encrypted;

            public TableFlags flags;
            public bool hasCdx;
            public bool hasMemos;
            public bool hasDbc;

            public CodepageId codepage;

            public dbf_header(dbf_head raw_hdr)
            {
                VfpFileType = (FoxProFileType)raw_hdr.dBASE_Info;

                byte currYrLastTwoDigits = (byte)(DateTime.Now.Year % 100);
                int fixedYr = raw_hdr.year;
                fixedYr += fixedYr <= currYrLastTwoDigits ? 2000 : 1900;
                lastWrite = new(fixedYr, raw_hdr.month, raw_hdr.day);

                recordCt = raw_hdr.recordCt;
                hdrLen = raw_hdr.hdrLen;
                recordStride = raw_hdr.recordStride;
                incompleteTransaction = raw_hdr.incompleteTransaction == BOOL.True;
                encrypted = raw_hdr.encrypted == BOOL.True;
                codepage = (CodepageId)raw_hdr.codepageId;

                flags = (TableFlags)raw_hdr.tableFlags;

                hasCdx = flags.HasFlag(TableFlags.HasCdx) |
                            flags.HasFlag(TableFlags.HasCdxAndDbc) |
                            flags.HasFlag(TableFlags.HasMemoAndCdx) |
                            flags.HasFlag(TableFlags.HasCdxAndMemoAndDbc);

                hasMemos = flags.HasFlag(TableFlags.HasMemo) |
                            flags.HasFlag(TableFlags.HasMemoAndCdx) |
                            flags.HasFlag(TableFlags.HasMemoAndDbc) |
                            flags.HasFlag(TableFlags.HasCdxAndMemoAndDbc);

                hasDbc = flags.HasFlag(TableFlags.HasDbc) |
                            flags.HasFlag(TableFlags.HasCdxAndDbc) |
                            flags.HasFlag(TableFlags.HasMemoAndDbc) |
                            flags.HasFlag(TableFlags.HasCdxAndMemoAndDbc);
            }
        }

        #endregion

        public const int DBF_HEAD_SZ_BYTES = 32;
        public const byte MAX_FIELD_LEN = 0xFE;
        public const byte FIELD_NULL_TERM = 0x0D;
        public const byte MAX_FIELD_CT = 0xFF;

        public string path;
        public string filename;

        public byte[]? rawData;
        public dbf_header info = new();
        public ColumnHeader[] headers = Array.Empty<ColumnHeader>();
        public RawRecord[] records = Array.Empty<RawRecord>();

        public RawFPTData? FptData = null;

        static readonly string TRUE = "true";
        static readonly string FALSE = "false";
        static readonly string MEMO_FMT = "-MEMO_ID-{0}";
        static readonly string SINGLE_QUOTE = "\"";
        static readonly string DOUBLE_QUOTE = SINGLE_QUOTE + SINGLE_QUOTE;
        static readonly string COMMA = ",";
        static readonly string COMMA_DELIM = "{0}" + COMMA;
        static readonly string CRLF = "\r\n";

        public static string GetCsvStr(string s)
        {
            if (s.Contains(SINGLE_QUOTE))
            {
                s = s.Replace(SINGLE_QUOTE, DOUBLE_QUOTE);
            }

            if (s.Contains(CRLF) || s.Contains(COMMA))
            {
                return string.Concat('\"', s, '\"');
            }

            return s;
        }

        public bool ToCsv(string csvPath)
        {
            if (Helper.InvalidFileDir(csvPath))
            {
                throw new ArgumentException($"Invalid path ({csvPath}) passed to {nameof(ToCsv)} function");
            }
            if (headers == null || headers.Length < 1)
            {
                return false;
            }

            StringBuilder sb = new(info.recordStride * 2);
            RawRecord rr;

            int hdrCt = headers.Length - 1;
            int recordCt = records.Length;
            string[] piped = new string[recordCt + 2];
            for (int hdr = 0; hdr < hdrCt; ++hdr)
            {
                sb.AppendFormat(COMMA_DELIM, headers[hdr].name);
            }
            sb.Append(headers[hdrCt].name);
            piped[0] = sb.ToString();
            sb.Clear();

            string str;
            for (int row = 0; row < recordCt; ++row, sb.Clear())
            {
                rr = records[row];

                for (int col = 0; col < hdrCt; ++col)
                {
                    str = GetCsvStr(rr.content[col]);
                    sb.AppendFormat(COMMA_DELIM, str);
                }

                str = GetCsvStr(rr.content[hdrCt]);
                sb.Append(str);

                piped[row + 1] = sb.ToString();
            }

            File.WriteAllLines(csvPath, piped);
            return File.Exists(csvPath);
        }

        public static string BytesToString(FieldType ft, byte* b, int len)
        {
            switch (ft)
            {
                case FieldType.Memo:

                    i32 id = new(*b++, *b++, *b++, *b);
                    return string.Format(MEMO_FMT, id.u32_full.ToString());

                case FieldType.Integer:

                    Span<char> intDigits = stackalloc char[len + 1];
                    for (int i = 0, j = 0; i < len; ++i, ++b)
                    {
                        if ((*b > 47 && *b <= 58) || *b == 45)
                        {
                            intDigits[j++] = (char)*b;
                        }
                    }
                    return int.Parse(intDigits).ToString();

                case FieldType.Num:
                case FieldType.Float:

                    Span<char> fltDigits = stackalloc char[len + 1];
                    for (int i = 0, j = 0; i < len; ++i, ++b)
                    {
                        if ((*b > 47 && *b <= 58) || *b == 45 || *b == 46)
                        {
                            fltDigits[j++] = (char)*b;
                        }
                    }
                    return float.Parse(fltDigits).ToString();

                case FieldType.Double:

                    Span<char> dblDigits = stackalloc char[len + 1];
                    for (int i = 0, j = 0; i < len; ++i, ++b)
                    {
                        if ((*b > 47 && *b <= 58) || *b == 45 || *b == 46)
                        {
                            dblDigits[j++] = (char)*b;
                        }
                    }
                    return double.Parse(dblDigits).ToString();

                case FieldType.Logic:

                    char truthVal = (char)*b;
                    return truthVal == 'T' ? TRUE : FALSE;

                case FieldType.Date:

                    // VFP Dates are formatted as: YYYY-MM-DD
                    char* date = stackalloc char[10] {  (char)*b++,
                                                        (char)*b++,
                                                        (char)*b++,
                                                        (char)*b++,
                                                        '-',
                                                        (char)*b++,
                                                        (char)*b++,
                                                        '-',
                                                        (char)*b++,
                                                        (char)*b };
                    return new string(date);

                default: break;
            }

            if (len < 1)
            {
                return string.Empty;
            }

            switch (len)
            {
                case 1:

                    char* one = stackalloc char[2] { (char)*b++, '\0' };
                    return new string(one, 0, 1);

                case 2:

                    char* two = stackalloc char[3] { (char)*b++, (char)*b, '\0' };
                    return new string(two, 0, 2);

                case 3:

                    char* three = stackalloc char[4] {  (char)*b++,
                                                        (char)*b++,
                                                        (char)*b, '\0' };
                    return new string(three, 0, 3);

                case 4:

                    char* four = stackalloc char[5] {   (char)*b++,
                                                        (char)*b++,
                                                        (char)*b++,
                                                        (char)*b, '\0' };
                    return new string(four, 0, 4);

                case 8:

                    char* eight = stackalloc char[9] {  (char)*b++,
                                                        (char)*b++,
                                                        (char)*b++,
                                                        (char)*b++,
                                                        (char)*b++,
                                                        (char)*b++,
                                                        (char)*b++,
                                                        (char)*b, '\0' };
                    return new string(eight, 0, 8);

                default:

                    char* ascii = stackalloc char[len + 1];
                    char* ascii_char = ascii;
                    for (int i = 0; i < len; ++i)
                    {
                        *ascii_char++ = (char)*b++;
                    }
                    return new string(ascii, 0, len);

            }
        }

        public string GetClassMap(string csName, bool props = true)
        {
            if (headers == null || headers.Length < 1)
            {
                return string.Empty;
            }

            int varCt = headers.Length;

            if (props)
            {
                CsProperty[] dbfProps = new CsProperty[varCt];

                for (int i = 0; i < varCt; ++i)
                {
                    dbfProps[i] = new CsProperty(AccessQualifier.Public,
                                                Modifier.NULL,
                                                headers[i].name,
                                                headers[i].csType);
                }

                return CsGen.ConstructClass(className: csName, fields: null, properties: dbfProps);
            }
            else
            {
                CsField[] dbfFields = new CsField[varCt];

                for (int i = 0; i < varCt; ++i)
                {
                    dbfFields[i] = new CsField(AccessQualifier.Public,
                                                Modifier.NULL,
                                                headers[i].name,
                                                headers[i].csType);
                }

                return CsGen.ConstructClass(csName, dbfFields, null);
            }
        }

        public bool ClassMapToFile(string csPath, bool props = true)
        {
            if (Helper.InvalidFileDir(csPath))
            {
                throw new ArgumentException($"Invalid DBF path ({csPath}) passed to {nameof(ClassMapToFile)}");
            }
            if (string.IsNullOrWhiteSpace(filename))
            {
                throw new ArgumentException($"Invalid DBF Filename of {nameof(RawDbfData)} object.");
            }

            try
            {
                FileInfo fi = new(csPath);
                File.WriteAllText(csPath, GetClassMap(fi.Name[0..^3], props));
                return File.Exists(csPath);
            }
            catch
            {
                return false;
            }
        }

        public static dbf_header GetHead(byte* dbfSrc)
        {
            dbf_head top = new();
            byte* dst = &top.dBASE_Info;
            Buffer.MemoryCopy(dbfSrc, dst, DBF_HEAD_SZ_BYTES, DBF_HEAD_SZ_BYTES);
            return new(top);
        }

        public ColumnHeader[] GetFieldHeaders(byte* dbfHandle)
        {
            List<ColumnHeader> hdrs = new();

            field_head* fh = (field_head*)(dbfHandle + DBF_HEAD_SZ_BYTES);
            byte* start = (byte*)fh;

            while (fh->name.c0 != FIELD_NULL_TERM)
            {
                byte flag = start[18];
                hdrs.Add(new ColumnHeader(*fh++));
                start = (byte*)fh;
            }

            return hdrs.ToArray();
        }

        public void GetRecords(byte* dbfHandle)
        {
            //const byte ACTIVE_RECORD = 0x20;
            const byte DELETED_RECORD = 0x2A;

            records = new RawRecord[info.recordCt];
            byte* src = dbfHandle + info.hdrLen;
            byte* dst = stackalloc byte[MAX_FIELD_CT];

            ColumnHeader currCol;
            RawRecord record;
            int byteLen;
            for (int row = 0, colCt = headers.Length; row < info.recordCt; ++row)
            {
                record = new(*src++ == DELETED_RECORD, colCt);

                for (int col = 0; col < colCt; ++col)
                {
                    currCol = headers[col];
                    byteLen = currCol.fieldLen;

                    Buffer.MemoryCopy(src, dst, MAX_FIELD_CT, byteLen);
                    record.content[col] = BytesToString(currCol.dbfType, dst, byteLen);
                    src += byteLen;
                }

                records[row] = record;
            }
        }

        public void GetRecordsWithMemos(byte* dbfHandle, RawFPTData fptData)
        {
            //const byte ACTIVE_RECORD = 0x20;
            const byte DELETED_RECORD = 0x2A;

            records = new RawRecord[info.recordCt];
            byte* src = dbfHandle + info.hdrLen;
            byte* dst = stackalloc byte[MAX_FIELD_CT];

            ColumnHeader currCol;
            RawRecord record;
            int byteLen;
            int id;
            for (int row = 0, colCt = headers.Length; row < info.recordCt; ++row)
            {
                record = new(*src++ == DELETED_RECORD, colCt);

                for (int col = 0; col < colCt; ++col)
                {
                    currCol = headers[col];
                    byteLen = currCol.fieldLen;

                    Buffer.MemoryCopy(src, dst, MAX_FIELD_CT, byteLen);

                    if (currCol.dbfType == FieldType.Memo)
                    {
                        id = new i32(src[0], src[1], src[2], src[3]).i32_full;
                        record.content[col] = fptData.GetMemoContent(id);
                    }
                    else
                    {
                        record.content[col] = BytesToString(currCol.dbfType, dst, byteLen).Trim();
                    }

                    src += byteLen;
                }

                records[row] = record;
            }
        }

        public RawDbfData(string dbfPath)
        {
            if (Helper.InvalidFileDir(dbfPath))
            {
                throw new ArgumentException($"Invalid DBF path ({dbfPath}) passed to {nameof(RawDbfData)} constructor");
            }

            FileInfo fi = new(dbfPath);
            path = fi.FullName;
            filename = fi.Name;
            rawData = File.ReadAllBytes(dbfPath);

            fixed (byte* fxd_data = rawData)
            {
                info = GetHead(fxd_data);
                headers = GetFieldHeaders(fxd_data);

                if (info.hasMemos)
                {
                    DirectoryInfo? parentDir = fi.Directory ?? throw new DirectoryNotFoundException($"DBF has memos, but the FPT directory is invalid.");

                    string fptFn = string.Concat(filename[..^4], ".fpt");
                    string fptPath = Path.Combine(parentDir.FullName, fptFn);
                    FptData = new(fptPath);

                    GetRecordsWithMemos(fxd_data, FptData);
                }
                else
                {
                    GetRecords(fxd_data);
                }
            }
        }

        public RawDbfData()
        {
            rawData = null;
            path = string.Empty;
            filename = string.Empty;
        }
    }
}
