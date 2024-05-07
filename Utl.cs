using System.IO;

namespace DbfDeserializer
{
    public static unsafe class Utl
    {
        private static readonly char[] TRIM_PATH = new char[] { '\\', '/' };

        /// <summary>
        /// This does not make any claims as to whether or not a string
        /// IS a path, but one can confidently determine if a path is
        /// NOT a path.
        /// </summary>
        public static bool CantBePath(this string p)
        {
            const int MAX_PATH_LEN = 255;
            int len = p.Length;
            return string.IsNullOrWhiteSpace(p) || len < 3 || len > MAX_PATH_LEN;
        }

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
                while (--len > -1)
                    hash = ((hash << 5) + hash) + *c++; // hash * 33 + c
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
            if (p.CantBePath()) return true;

            try
            {
                string fullPath = Path.GetFullPath(p);
                string? root = Path.GetPathRoot(p);
                if (string.IsNullOrEmpty(root)) return false;
                else return string.IsNullOrEmpty(root.Trim(TRIM_PATH));
            }
            catch { return true; }
        }

        /// <summary>
        ///
        /// Determines whether or not an input directory path is invalid.
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
        public static bool InvalidAbsDir(string p)
        {
            if (InvalidAbsPath(p)) return true;

            try
            {
                FileAttributes fa = File.GetAttributes(p);
                return (fa & FileAttributes.Directory) != FileAttributes.Directory ||
                        Directory.Exists(p) == false;
            }
            catch { return true; }
        }

        /// <summary>
        ///
        /// Determines whether or not an input FILE path is invalid.
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
        public static bool InvalidAbsFileDir(string p)
        {
            string? dir = Path.GetDirectoryName(p);
            return string.IsNullOrWhiteSpace(dir) || InvalidAbsDir(dir);
        }
    }
}
