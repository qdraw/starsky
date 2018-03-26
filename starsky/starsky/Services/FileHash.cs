using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace starsky.Services
{
    public class FileHash
    {
        public static List<string> GetHashCode(string[] filesInDirectoryFullPath)
        {
            return filesInDirectoryFullPath.Select(fileFullPath => CalcHashCode(fileFullPath)).ToList();
        }

        public static string GetHashCode(string filename)
        {
            return CalcHashCode(filename);
        }

        public static string CalcHashCode(string filename)
        {
            var q = WrapSomeMethod(filename).Result;
//            Console.WriteLine(q);
            return q;

        }

        private static async Task<string> WrapSomeMethod(string someParam)
        {
            //adding .ConfigureAwait(false) may NOT be what you want but google it.
            return await Task.Run(() => Md5Timeout(someParam)).ConfigureAwait(false);
        }

        // Ignore Error CS1998
        #pragma warning disable 1998
        private static async Task<string> Md5Timeout(string fullFileName){
        #pragma warning restore 1998

            var task = Task.Run(() => CalculateMd5Async(fullFileName));
                if (task.Wait(TimeSpan.FromSeconds(20)))
                return task.Result;

            Console.WriteLine(">>>>>>>>>>>            Timeout Md5 Hashing::: "
                              + fullFileName 
                              + "            <<<<<<<<<<<<");
            
            return Base32.Encode(GenerateRandomBytes(25));
            throw new Exception("Timed out");
        }

        public static byte[] GenerateRandomBytes(int length)
        {
            // Create a buffer
            byte[] randBytes;
  
            if (length >= 1)
            {
                randBytes = new byte[length];
            }
            else
            {
                randBytes = new byte[1];
            }
 
            // Create a new RNGCryptoServiceProvider.
            System.Security.Cryptography.RNGCryptoServiceProvider rand = 
                new System.Security.Cryptography.RNGCryptoServiceProvider();
 
            // Fill the buffer with random bytes.
            rand.GetBytes(randBytes);
 
            // return the bytes.
            return randBytes;
        }

        private static async Task<string> CalculateMd5Async(string fullFileName)
        {
            var block = ArrayPool<byte>.Shared.Rent(8192);
            try
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = new FileStream(fullFileName, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, true))
                    {
                        int length;
                        while ((length = await stream.ReadAsync(block, 0, block.Length).ConfigureAwait(false)) > 0)
                        {
                            md5.TransformBlock(block, 0, length, null, 0);
                        }
                        md5.TransformFinalBlock(block, 0, 0);
                    }
                    var hash = md5.Hash;       
                    return Base32.Encode(hash);
//                    return Convert.ToBase64String(hash);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(block);
            }
        }
        

//        public static string CalcHashCode(BufferedStream filestream)
//        {
////            SHA256Managed sha = new SHA256Managed();
////            byte[] hash = sha.ComputeHash(filestream);
//            MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider();
//            Byte[] hash = md5Provider.ComputeHash(filestream);
//
//            if (AppSettingsProvider.Verbose) Console.WriteLine(hash);
//
//            var stringHash =   Base32.Encode(hash);
//
//            stringHash = stringHash.Replace("/", "_");
//            stringHash = stringHash.Replace("\\", "_");
//            stringHash = stringHash.Replace("==", "");
//            stringHash = stringHash.Replace("+", "0");
//            if (AppSettingsProvider.Verbose) Console.WriteLine(stringHash);
//
//            return stringHash;
//        }
    }
}
