using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Rewrite.Internal.ApacheModRewrite;
using Microsoft.Extensions.Configuration;
using starsky.Models;

namespace starsky.Services
{
    public class Files
    {
        public string CheckMd5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    return Encoding.Default.GetString(md5.ComputeHash(stream));
                }
            }
        }

        public static IEnumerable<FileIndexItem> GetFiles()
        {
            var path = AppSettingsProvider.BasePath;

            Queue<string> queue = new Queue<string>();
            queue.Enqueue(path);
            while (queue.Count > 0)
            {
                path = queue.Dequeue();
                try
                {
                    foreach (string subDir in Directory.GetDirectories(path))
                    {
                        queue.Enqueue(subDir);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
                string[] files = null;
                try
                {
                    files = Directory.GetFiles(path);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
                if (files != null)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        if (files[i].Contains(".jpg"))
                        {
                            var fileItem = new FileIndexItem
                            {
                                FilePath = files[i],
                                FileName = Path.GetFileName(files[i]),
                                FileHash = CalcHashCode(files[i])
                            };
                            yield return fileItem;
                        }
                    }
                }
            }
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
            MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider();
            Byte[] hash = md5Provider.ComputeHash(file);
            return Convert.ToBase64String(hash);
        }

        public static string PathToUnixStyle(string filepath)
        {
            filepath = filepath.Replace(AppSettingsProvider.BasePath, "");

            if (Path.DirectorySeparatorChar.ToString() == "\\")
            {
                filepath = filepath.Replace("\\", "/");
            }
            return filepath;
        }

        public static string PathToFull(string shortPath)
        {
            var filepath = AppSettingsProvider.BasePath + shortPath;
            if (Path.DirectorySeparatorChar.ToString() == "\\")
            {
                filepath = filepath.Replace("\\", "/");
            }

            return File.Exists(filepath) ? filepath : null;
        }
    }
}
