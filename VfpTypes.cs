using System;
using System.Runtime.InteropServices;

namespace DbfDeserializer
{
    public enum Bool : byte
    {
        False = 0,
        True = 1
    }

    public enum FoxproFileType : byte
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

    public enum DbfFieldType : byte
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

    public enum BlockSignature : int
    {
        Image = 0, Text, Object
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Size = 10, Pack = 1)]
    public struct TenBytesReserved
    {
        public byte r0; public byte r1;
        public byte r2; public byte r3;
        public byte r4; public byte r5;
        public byte r6; public byte r7;
        public byte r8; public byte r9;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 4, CharSet = CharSet.Ansi)]
    public struct I32
    {
        [FieldOffset(0)] public int i32_full;
        [FieldOffset(0)] public uint u32_full;
        [FieldOffset(0)] public float f32_full;

        [FieldOffset(0)] public short i16_top;
        [FieldOffset(2)] public short i16_bot;
        [FieldOffset(0)] public short u16_top;
        [FieldOffset(2)] public short u16_bot;

        [FieldOffset(0)] public byte b0;
        [FieldOffset(1)] public byte b1;
        [FieldOffset(2)] public byte b2;
        [FieldOffset(3)] public byte b3;

        public I32(byte _b0, byte _b1, byte _b2, byte _b3)
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

        public static int B2I(byte _b0, byte _b1, byte _b2, byte _b3)
            => new I32(_b0, _b1, _b2, _b3).i32_full;
        public static uint B2U(byte _b0, byte _b1, byte _b2, byte _b3)
            => new I32(_b0, _b1, _b2, _b3).u32_full;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 2, CharSet = CharSet.Ansi)]
    public struct I16
    {
        [FieldOffset(0)] public short full;
        [FieldOffset(0)] public ushort ufull;

        [FieldOffset(0)] public byte b0;
        [FieldOffset(1)] public byte b1;

        public I16(byte _b0, byte _b1)
        {
            full = 0;
            ufull = 0;

            b0 = _b0;
            b1 = _b1;
        }

        public static short B2I(byte _b0, byte _b1) => new I16(_b0, _b1).full;
        public static ushort B2U(byte _b0, byte _b1) => new I16(_b0, _b1).ufull;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Size = 10, Pack = 1)]
    public struct FieldName
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
    public struct FieldHead
    {
        public FieldName name;
        public DbfFieldType dbfType;
        public int fieldRecPos; // Displacement of field in record
        public byte fieldLen;
        public byte fieldDecimalCt;
        public FieldFlags flags;
        public int nextIncVal;
        public byte nextStepVal;
        public long reserved_8;
    }

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
    public struct DosMultiUserEnv
    {
        [FieldOffset(0)] public byte DOS_MU_ENV_0;
        [FieldOffset(1)] public byte DOS_MU_ENV_1;
        [FieldOffset(2)] public byte DOS_MU_ENV_2;
        [FieldOffset(3)] public byte DOS_MU_ENV_3;
        [FieldOffset(4)] public byte DOS_MU_ENV_4;
        [FieldOffset(5)] public byte DOS_MU_ENV_5;
        [FieldOffset(6)] public byte DOS_MU_ENV_6;
        [FieldOffset(7)] public byte DOS_MU_ENV_7;
        [FieldOffset(8)] public byte DOS_MU_ENV_8;
        [FieldOffset(9)] public byte DOS_MU_ENV_9;
        [FieldOffset(10)] public byte DOS_MU_ENV_10;
        [FieldOffset(11)] public byte DOS_MU_ENV_11;

        public DosMultiUserEnv(byte _b0, byte _b1, byte _b2, byte _b3, byte _b4,
                            byte _b5, byte _b6, byte _b7, byte _b8, byte _b9,
                            byte _b10, byte _b11)
        {
            DOS_MU_ENV_0 = _b0;     DOS_MU_ENV_1 = _b1;
            DOS_MU_ENV_2 = _b2;     DOS_MU_ENV_3 = _b3;
            DOS_MU_ENV_4 = _b4;     DOS_MU_ENV_5 = _b5;
            DOS_MU_ENV_6 = _b6;     DOS_MU_ENV_7 = _b7;
            DOS_MU_ENV_8 = _b8;     DOS_MU_ENV_9 = _b9;
            DOS_MU_ENV_10 = _b10;   DOS_MU_ENV_11 = _b11;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi, Size = 3)]
    public struct DbfDate
    {
        [MarshalAs(UnmanagedType.I1)] public byte year;
        [MarshalAs(UnmanagedType.I1)] public byte month;
        [MarshalAs(UnmanagedType.I1)] public byte day;

        public DbfDate(byte _year, byte _month, byte _day)
        {
            year = _year;
            month = _month;
            day = _day;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Size = 32)]
    public struct DbfHead
    {
        public byte dBASE_Info;

        public byte year;
        public byte month;
        public byte day;

        public int recordCt;
        public short hdrLen;
        public short recordStride;

        private readonly short RESERVED_0;

        public Bool incompleteTransaction;
        public Bool encrypted;
        public DosMultiUserEnv MultiuserEnv;
        public byte tableFlags;
        public byte codepageId;

        private readonly short RESERVED_EOF;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct DbfHdr
    {
        public FoxproFileType VfpFileType;
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

        public DbfHdr(DbfHead raw_hdr)
        {
            VfpFileType = (FoxproFileType)raw_hdr.dBASE_Info;

            byte currYrLastTwoDigits = (byte)(DateTime.Now.Year % 100);
            int fixedYr = raw_hdr.year;
            fixedYr += fixedYr <= currYrLastTwoDigits ? 2000 : 1900;
            lastWrite = new(fixedYr, raw_hdr.month, raw_hdr.day);

            recordCt = raw_hdr.recordCt;
            hdrLen = raw_hdr.hdrLen;
            recordStride = raw_hdr.recordStride;
            incompleteTransaction = raw_hdr.incompleteTransaction == Bool.True;
            encrypted = raw_hdr.encrypted == Bool.True;
            codepage = (CodepageId)raw_hdr.codepageId;

            flags = (TableFlags)raw_hdr.tableFlags;

            hasCdx =    flags.HasFlag(TableFlags.HasCdx) |
                        flags.HasFlag(TableFlags.HasCdxAndDbc) |
                        flags.HasFlag(TableFlags.HasMemoAndCdx) |
                        flags.HasFlag(TableFlags.HasCdxAndMemoAndDbc);

            hasMemos =  flags.HasFlag(TableFlags.HasMemo) |
                        flags.HasFlag(TableFlags.HasMemoAndCdx) |
                        flags.HasFlag(TableFlags.HasMemoAndDbc) |
                        flags.HasFlag(TableFlags.HasCdxAndMemoAndDbc);

            hasDbc =    flags.HasFlag(TableFlags.HasDbc) |
                        flags.HasFlag(TableFlags.HasCdxAndDbc) |
                        flags.HasFlag(TableFlags.HasMemoAndDbc) |
                        flags.HasFlag(TableFlags.HasCdxAndMemoAndDbc);
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public unsafe struct MemoHdr
    {
        public int freeBlockOffset;
        public ushort bytesPerBlock;
        public int endOffset;

        public MemoHdr(in byte* data)
        {
            // NOTE(RYAN_2023-10-25):   FPT encodes this data in big endian.
            //                          Need to swap the bytes as a
            //                          consequence.
            freeBlockOffset = I32.B2I(data[3], data[2], data[1], data[0]);
            bytesPerBlock = I16.B2U(data[7], data[6]);

            endOffset = freeBlockOffset * bytesPerBlock;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Ansi)]
    public unsafe struct MemoBlockHdr
    {
        public BlockSignature DataType;
        public int blockOffset;
        public int memoByteLen;
        public int blockCt;
        public int blockStride;

        public MemoBlockHdr(in byte* data, int blockSzBtyeLen, int offset)
        {
            const int MEMO_BLOCK_HEAD_SZ = VfpTypes.MEMO_BLOCK_HEADER_BYTE_SZ;
            blockOffset = offset;

            // NOTE(RYAN_2023-10-25):   FPT encodes this data in big
            //                          endian. Need to swap the bytes
            //                          as a consequence.
            DataType = (BlockSignature)I32.B2I(data[3], data[2], data[1], data[0]);
            memoByteLen = I32.B2I(data[7], data[6], data[5], data[4]);
            blockCt = 1;

            // NOTE(RYAN_2024-03-14):   Take note that the header is
            //                          INCLUDED in the block size. So
            //                          every block is:
            //
            //                          { blockSz - header_size (8 bytes) }
            //
            int leftoverBytes = blockSzBtyeLen - MEMO_BLOCK_HEAD_SZ;
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

    public static class VfpTypes
    {
        public const int MEMO_BLOCK_HEADER_BYTE_SZ = 8;
    }
}
