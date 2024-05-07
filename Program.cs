using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

using static DbfDeserializer.VfpTypes;

namespace DbfDeserializer
{
    public static unsafe class Helper
    {
        public unsafe class RawFptData
        {
            public class Memo
            {
                public MemoBlockHdr header;
                public string content = string.Empty;

                public Memo(in byte* blockStart, int memoBlockLen, int offset)
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

            public MemoHdr memoHead;
            public byte[] data = Array.Empty<byte>();
            public Dictionary<int, Memo> Memos = new();

            public string GetMemoContent(int id)
            {
                int offset = id * memoHead.bytesPerBlock;
                if (Memos.TryGetValue(offset, out Memo? res))
                    return res.content;

                return string.Empty;
            }

            public void GetMemos(in byte* fxd_src)
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

            public RawFptData(string fptPath)
            {
                if (Utl.InvalidAbsFileDir(fptPath))
                    throw new ArgumentException($"Invalid FPT path ({fptPath}) passed to {nameof(RawFptData)} constructor");

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
            public class RawRecord
            {
                public bool deleted = false;
                public string[] content = Array.Empty<string>();

                public RawRecord() { }
                public RawRecord(bool recordDeleted, int columnCount)
                {
                    deleted = recordDeleted;
                    content = new string[columnCount];
                }
            }

            public class ColumnHeader
            {
                public string name = string.Empty;
                public DbfFieldType dbfType = DbfFieldType.General;
                public Type csType = typeof(string);
                public int fieldRecPos = -1; // Displacement of field in record
                public byte fieldLen = 0;
                public byte fieldDecimalCt = 0;
                public FieldFlags flags = 0;
                public int nextIncVal = -1;
                public byte nextStepVal = 0;

                public static Type DbfFieldToType(DbfFieldType ft)
                {
                    return ft switch
                    {
                        DbfFieldType.Char => typeof(string),
                        DbfFieldType.DateAndTime or DbfFieldType.Date => typeof(DateTime),
                        DbfFieldType.Float => typeof(float),
                        DbfFieldType.Logic => typeof(bool),
                        DbfFieldType.Memo => typeof(uint),
                        DbfFieldType.Currency => typeof(decimal),
                        DbfFieldType.Double or DbfFieldType.Num => typeof(double),
                        DbfFieldType.Integer => typeof(int),
                        _ => typeof(string),
                    };
                }

                public ColumnHeader() { }
                public ColumnHeader(FieldHead hdr)
                {
                    name = hdr.name.ToString();
                    dbfType = hdr.dbfType;
                    csType = DbfFieldToType(dbfType);
                    fieldRecPos = hdr.fieldRecPos;
                    fieldLen = hdr.fieldLen;
                    fieldDecimalCt = hdr.fieldDecimalCt;
                    flags = hdr.flags;
                    nextIncVal = hdr.nextIncVal;
                    nextStepVal = hdr.nextStepVal;
                }
            }

            public const int DBF_HEAD_SZ_BYTES = 32;
            public const byte MAX_FIELD_LEN = 0xFE;
            public const byte FIELD_NULL_TERM = 0x0D;
            public const byte MAX_FIELD_CT = 0xFF;

            public string path = string.Empty;
            public string filename = string.Empty;

            public byte[]? rawData = null;
            public DbfHdr info = new();
            public ColumnHeader[] headers = Array.Empty<ColumnHeader>();
            public RawRecord[] records = Array.Empty<RawRecord>();

            public RawFptData? FptData = null;

            static readonly string TRUE = "true";
            static readonly string FALSE = "false";
            static readonly string MEMO_FMT = "-MEMO_ID-{0}";
            static readonly string SINGLE_QUOTE = "\"";
            static readonly string DOUBLE_QUOTE = SINGLE_QUOTE + SINGLE_QUOTE;
            static readonly string COMMA = ",";
            static readonly string COMMA_DELIM = "{0}" + COMMA;
            static readonly string CRLF = "\r\n";

            public void GetRecords(in byte* dbfHandle)
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

            public void GetRecordsWithMemos(in byte* dbfHandle, RawFptData fpt)
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

                        if (currCol.dbfType == DbfFieldType.Memo)
                        {
                            id = new I32(src[0], src[1], src[2], src[3]).i32_full;
                            record.content[col] = fpt.GetMemoContent(id);
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

            public static DbfHdr GetHead(in byte* dbfHandle)
            {
                DbfHead top = new();
                byte* dst = &top.dBASE_Info;
                Buffer.MemoryCopy(dbfHandle, dst, DBF_HEAD_SZ_BYTES, DBF_HEAD_SZ_BYTES);
                return new(top);
            }

            public ColumnHeader[] GetFieldHeaders(in byte* dbfHandle)
            {
                List<ColumnHeader> hdrs = new();

                FieldHead* fh = (FieldHead*)(dbfHandle + DBF_HEAD_SZ_BYTES);
                byte* start = (byte*)fh;

                while (fh->name.c0 != FIELD_NULL_TERM)
                {
                    byte flag = start[18];
                    hdrs.Add(new ColumnHeader(*fh++));
                    start = (byte*)fh;
                }

                return hdrs.ToArray();
            }

            public RawDbfData() { }
            public RawDbfData(string dbfPath)
            {
                if (Utl.InvalidAbsFileDir(dbfPath))
                    throw new ArgumentException($"Invalid DBF path ({dbfPath}) passed to {nameof(RawDbfData)} constructor");

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
                        GetRecords(fxd_data);
                }
            }

            public static string GetCsvStr(string s)
            {
                if (s.Contains(SINGLE_QUOTE))
                    s = s.Replace(SINGLE_QUOTE, DOUBLE_QUOTE);

                if (s.Contains(CRLF) || s.Contains(COMMA))
                    return string.Concat('\"', s, '\"');

                return s;
            }

            public bool ToCsv(string csvPath)
            {
                if (Utl.InvalidAbsFileDir(csvPath))
                    throw new ArgumentException($"Invalid path ({csvPath}) passed to {nameof(ToCsv)} function");

                if (headers == null || headers.Length < 1)
                    return false;

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

            public static string BytesToString(DbfFieldType ft, byte* b, int len)
            {
                switch (ft)
                {
                    case DbfFieldType.Memo:

                        I32 id = new(*b++, *b++, *b++, *b);
                        return string.Format(MEMO_FMT, id.u32_full.ToString());

                    case DbfFieldType.Integer:

                        Span<char> intDigits = stackalloc char[len + 1];
                        for (int i = 0, j = 0; i < len; ++i, ++b)
                        {
                            if ((*b > 47 && *b <= 58) || *b == 45)
                            {
                                intDigits[j++] = (char)*b;
                            }
                        }
                        return int.Parse(intDigits).ToString();

                    case DbfFieldType.Num:
                    case DbfFieldType.Float:

                        Span<char> fltDigits = stackalloc char[len + 1];
                        for (int i = 0, j = 0; i < len; ++i, ++b)
                        {
                            if ((*b > 47 && *b <= 58) || *b == 45 || *b == 46)
                            {
                                fltDigits[j++] = (char)*b;
                            }
                        }
                        return float.Parse(fltDigits).ToString();

                    case DbfFieldType.Double:

                        Span<char> dblDigits = stackalloc char[len + 1];
                        for (int i = 0, j = 0; i < len; ++i, ++b)
                        {
                            if ((*b > 47 && *b <= 58) || *b == 45 || *b == 46)
                            {
                                dblDigits[j++] = (char)*b;
                            }
                        }
                        return double.Parse(dblDigits).ToString();

                    case DbfFieldType.Logic:

                        char truthVal = (char)*b;
                        return truthVal == 'T' ? TRUE : FALSE;

                    case DbfFieldType.Date:

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
                    return string.Empty;

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
        }
    }
}
