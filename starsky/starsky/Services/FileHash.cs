using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace starsky.Services
{
    public class FileHash
    {
        public static List<string> CalcHashCode(string[] filesInDirectoryFullPath)
        {
            return filesInDirectoryFullPath.Select(fileFullPath => CalcHashCode(fileFullPath)).ToList();
        }

        public static string CalcHashCode(string filename)
        {
            FileStream stream = new FileStream(
                filename,
                System.IO.FileMode.Open,
                System.IO.FileAccess.Read,
                System.IO.FileShare.ReadWrite);

            try
            {
                return CalcHashCode(stream);
            }
            finally
            {
                stream.Close();
            }
        }

        public static string CalcHashCode(FileStream file)
        {
            //using (var stream = new BufferedStream(file, 1200000))
            //{
            //    SHA256Managed sha = new SHA256Managed();
            //    byte[] checksum = sha.ComputeHash(stream);
            //    return BitConverter.ToString(checksum).Replace("-", String.Empty).ToLower();
            //}

            MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider();
            Byte[] hash = md5Provider.ComputeHash(file);

            var stringHash = Base32.Encode(hash);

            stringHash = stringHash.Replace("/", "_");
            stringHash = stringHash.Replace("\\", "_");
            stringHash = stringHash.Replace("==", "");
            stringHash = stringHash.Replace("+", "0");

            return stringHash;

        }
    }
}
