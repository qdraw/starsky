using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace starskycore.Services
{
	[Obsolete]
    public static class FileHashStatic
    {
        // Two public interfaces
        // Returns list of hashcodes
        // or one hashcode (base32)
        
        
        /// <summary>
        /// Get the hashcodes of a array of files
        /// Uses the default timeout
        /// </summary>
        /// <param name="filesInDirectoryFullPath">array</param>
        /// <returns>array of base32 hashes</returns>
        public static List<string> GetHashCode(string[] filesInDirectoryFullPath)
        {
            return filesInDirectoryFullPath.Select(fileFullPath => _calcHashCode(fileFullPath)).ToList();
        }

        /// <summary>
        /// Returns a Base32 case insensitive filehash, used with the default timeout of 8 seconds
        /// </summary>
        /// <param name="filename">FullFilePath</param>
        /// <param name="timeoutSeconds">Timeout in seconds, before a random string will be returned</param>
        /// <returns>base32 hash</returns>
        public static string GetHashCode(string filename, int timeoutSeconds = 8)
        {
            return _calcHashCode(filename,timeoutSeconds);
        }
        
        // Here are some tricks used to avoid that CalculateMd5Async keeps waiting forever.
        // In some cases hashing a file keeps waiting forever (at least on linux-arm)

        private static string _calcHashCode(string filename, int timeoutSeconds = 8)
        {
            var q = Md5TimeoutAsyncWrapper(filename,timeoutSeconds).Result;
            return q;
        }

        // Wrapper to do Async tasks -- add variable to test make it in a unit test shorter
        private static async Task<string> Md5TimeoutAsyncWrapper(string fullFileName, int timeoutSeconds)
        {
            //adding .ConfigureAwait(false) may NOT be what you want but google it.
            return await Task.Run(() => Md5Timeout(fullFileName,timeoutSeconds)).ConfigureAwait(false);
        }

        // Yes I know that I don't use the class propper, I use it in a sync way.
        #pragma warning disable 1998
        private static async Task<string> Md5Timeout(string fullFileName, int timeoutSeconds){
        #pragma warning restore 1998

            var task = Task.Run(() => CalculateMd5Async(fullFileName));
                if (task.Wait(TimeSpan.FromSeconds(timeoutSeconds)))
                return task.Result;

            // Sometimes a Calc keeps waiting for days
            Console.WriteLine(">>>>>>>>>>>            Timeout Md5 Hashing::: "
                              + fullFileName 
                              + "            <<<<<<<<<<<<");
            
            return Base32.Encode(GenerateRandomBytes(27)) + "_T";
        }

        // Create a random string
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

        /// <summary>
        ///  Calculate the hash based on the first 0.05 Mb of the file
        /// </summary>
        /// <param name="fullFileName"></param>
        /// <returns></returns>
        private static async Task<string> CalculateMd5Async(string fullFileName)
        {
            var block = ArrayPool<byte>.Shared.Rent(50000); // 0,05 Mb
            try
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = new FileStream(fullFileName, 
                        FileMode.Open, FileAccess.Read, FileShare.Read, 50000, true))
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
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(block);
            }
        }
    }
}
