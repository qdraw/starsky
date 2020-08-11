using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using starsky.foundation.platform.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;

namespace starskytest.FakeMocks
{
	class FakeIStorage : IStorage
	{
		private List<string> _outputSubPathFolders = new List<string>();
		private List<string> _outputSubPathFiles  = new List<string>();
		private readonly  Dictionary<string, byte[]> _byteList = new Dictionary<string, byte[]>();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="outputSubPathFolders">/</param>
		/// <param name="outputSubPathFiles">/test.jpg</param>
		/// <param name="byteListSource"></param>
		/// <param name="fileHashPerThumbnail">for mock fileHash=subPath</param>
		public FakeIStorage(List<string> outputSubPathFolders = null, List<string> outputSubPathFiles = null, 
			IReadOnlyList<byte[]> byteListSource = null)
		{
	
			if ( outputSubPathFolders != null )
			{
				foreach ( var subPath in outputSubPathFolders )
				{
					_outputSubPathFolders.Add(PathHelper.PrefixDbSlash(subPath));
				}
			}

			if ( outputSubPathFiles != null )
			{
				foreach ( var subPath in outputSubPathFiles )
				{
					_outputSubPathFiles.Add(PathHelper.PrefixDbSlash(subPath));
				}
			}

			if ( byteListSource != null && byteListSource.Count == _outputSubPathFiles.Count)
			{
				for ( int i = 0; i < _outputSubPathFiles.Count; i++ )
				{
					_byteList.Add(_outputSubPathFiles[i],byteListSource[i]);
				}
			}

		}

		private readonly Exception _exception;
		
		public FakeIStorage(Exception exception)
		{
			_exception = exception;
		}
		
		public bool ExistFile(string path)
		{
			return _outputSubPathFiles.Contains(PathHelper.PrefixDbSlash(path));
		}
		public bool ExistFolder(string path)
		{
			if ( _exception != null ) throw _exception;
			return _outputSubPathFolders.Contains(PathHelper.PrefixDbSlash(path));
		}

		public FolderOrFileModel.FolderOrFileTypeList IsFolderOrFile(string subPath = "")
		{
			if ( ExistFile(subPath) )
			{
				return FolderOrFileModel.FolderOrFileTypeList.File;
			}

			if ( ExistFolder(subPath) )
			{
				return FolderOrFileModel.FolderOrFileTypeList.Folder;
			}

			return FolderOrFileModel.FolderOrFileTypeList.Deleted;
		}

		public void FolderMove(string inputSubPath, string toSubPath)
		{
			var indexOfFolders = _outputSubPathFolders.IndexOf(inputSubPath);
			if ( indexOfFolders == -1 )
			{
				throw new ArgumentException($"inputSubPath:{inputSubPath} - toSubPath:{toSubPath} indexOfFolders---1");
			}
			_outputSubPathFolders[indexOfFolders] = toSubPath;
		}

		public void FileMove(string inputSubPath, string toSubPath)
		{
			inputSubPath = PathHelper.PrefixDbSlash(inputSubPath);
			toSubPath = PathHelper.PrefixDbSlash(toSubPath);

			var indexOfFiles = _outputSubPathFiles.IndexOf(inputSubPath);
			if ( indexOfFiles == -1 )
			{
				throw new ArgumentException($"inputSubPath:{inputSubPath} - toSubPath:{toSubPath} indexOfFiles---1");
			}
			_outputSubPathFiles[indexOfFiles] = toSubPath;
		}

		public void FileCopy(string fromPath, string toPath)
		{
			fromPath = PathHelper.PrefixDbSlash(fromPath);
			toPath = PathHelper.PrefixDbSlash(toPath);
			if ( !ExistFile(fromPath) ) return;
			
			_outputSubPathFiles.Add(toPath);
			_byteList.Add(toPath,_byteList[fromPath]);
		}

		public bool FileDelete(string path)
		{
			path = PathHelper.PrefixDbSlash(path);
			if ( !ExistFile(path) ) return false;
			var index = _outputSubPathFiles.IndexOf(path);
			_outputSubPathFiles[index] = null;
			return true;
		}

		public void CreateDirectory(string subPath)
		{
			_outputSubPathFolders.Add(subPath);
		}

		public bool FolderDelete(string path)
		{
			path = PathHelper.PrefixDbSlash(path);
			if ( !ExistFolder(path) ) return false;
			var index = _outputSubPathFolders.IndexOf(path);
			_outputSubPathFolders[index] = null;
			return true;
		}

		public IEnumerable<string> GetAllFilesInDirectory(string subPath)
		{
			subPath = PathHelper.RemoveLatestSlash(subPath);

			// non recruisive
			if ( subPath != string.Empty && !ExistFolder(subPath) )
			{
				return new List<string>();
			}

			return _outputSubPathFiles.Where(p => CheckAndFixParentFiles(subPath, p)).AsEnumerable();
		}

		public IEnumerable<string> GetAllFilesInDirectoryRecursive(string subPath)
		{
			subPath = PathHelper.RemoveLatestSlash(subPath);
			return _outputSubPathFiles.Where(p => p != null && p.StartsWith(subPath));
		}

		/// <summary>
		/// Should output: /2020/01/2020_01_01 and /2020/01/2020_01_01 test
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public IEnumerable<string> GetDirectories(string path)
		{
			
			path = PathHelper.RemoveLatestSlash(path);
			var folderFileList = _outputSubPathFolders.
				Where(p => p.Contains(path) && p != path).
				ToList();
			
			var parentPathWithSlash = string.IsNullOrEmpty(path) ? "/" : path;

			var folderFileListNotRecrusive = folderFileList.Where(p => CheckAndFixChildFolders(parentPathWithSlash, p)).ToList();
			return folderFileListNotRecrusive;
		}

		private bool CheckAndFixChildFolders(string parentFolder, string childFolder)
		{
			return Regex.Match(childFolder, $"^{Regex.Escape(PathHelper.AddSlash(parentFolder))}[^/]+$").Success;
		}

		private bool CheckAndFixParentFiles(string parentFolder, string filePath)
		{
			if ( parentFolder != string.Empty && !filePath.StartsWith(parentFolder) ) return false;
			// unescaped: (\/|\\)\w+.[a-z]{1,4}$
			return Regex.Match(filePath, $"^{Regex.Escape(parentFolder)}"+ "(\\/|\\\\)\\w+.[a-z]{1,4}$").Success;
		}

		public IEnumerable<string> GetDirectoryRecursive(string subPath)
		{
			subPath = PathHelper.RemoveLatestSlash(subPath);
			if ( !ExistFolder(subPath) )
			{
				return new List<string>();
			}
			return _outputSubPathFolders.Where(p => p.StartsWith(subPath) && p != subPath ).AsEnumerable();

		}

		public Stream ReadStream(string path, int maxRead = 2147483647)
		{
			path = PathHelper.PrefixDbSlash(path);
			
			if ( ExistFile(path) && _byteList.All(p => p.Key != path) )
			{
				byte[] byteArray = Encoding.UTF8.GetBytes("test");
				MemoryStream stream = new MemoryStream(byteArray);
				return stream;
			}
			if ( !ExistFile(path) ) throw new FileNotFoundException($"{path} is not found in FakeStorage");

			var result = _byteList.FirstOrDefault(p => p.Key == path).Value;
			MemoryStream stream1 = new MemoryStream(result);
			return stream1;
		}

		public bool WriteStreamOpenOrCreate(Stream stream, string path)
		{
			path = PathHelper.PrefixDbSlash(path);

			if ( !_outputSubPathFiles.Contains(path) )
			{
				_outputSubPathFiles.Add(path);
			}
			
			stream.Seek(0, SeekOrigin.Begin);

			using ( MemoryStream ms = new MemoryStream() )
			{
				stream.CopyTo(ms);
				var byteArray =  ms.ToArray();

				if ( _byteList.Any(p => p.Key == path) )
				{
					_byteList[path] = byteArray;
					return true;
				}
				
				_byteList.Add(path, byteArray);
				if ( byteArray.Length == 0 ) throw new FileLoadException($"FakeIStorage WriteStream => path {path} is 0 bytes");
			}
			return true;
		}

		public bool WriteStream(Stream stream, string path)
		{
			path = PathHelper.PrefixDbSlash(path);

			_outputSubPathFiles.Add(path);

			stream.Seek(0, SeekOrigin.Begin);

			using (MemoryStream ms = new MemoryStream())
			{
				stream.CopyTo(ms);
				var byteArray =  ms.ToArray();

				if ( _byteList.Any(p => p.Key == path) )
				{
					_byteList[path] = byteArray;
					return true;
				}
				
				_byteList.Add(path, byteArray);
				if ( byteArray.Length == 0 ) throw new FileLoadException($"FakeIStorage WriteStream => path {path} is 0 bytes");
			}
			stream.Dispose();
			return true;
		}

		public Task<bool> WriteStreamAsync(Stream stream, string path)
		{
			return Task.FromResult(WriteStream(stream, path));
		}

		public StorageInfo Info(string path)
		{
			path = PathHelper.PrefixDbSlash(path);
			if ( ExistFolder(path) )
			{
				return new StorageInfo
                {
                	IsFolderOrFile = FolderOrFileModel.FolderOrFileTypeList.Folder
                };
			}

			if ( !ExistFile(path) )
			{
				return new StorageInfo
				{
					IsFolderOrFile = FolderOrFileModel.FolderOrFileTypeList.Deleted
				};
			}
			
			var result = _byteList.FirstOrDefault(p => p.Key == path).Value;
			return new StorageInfo
			{
				IsFolderOrFile = FolderOrFileModel.FolderOrFileTypeList.File,
				Size = result.Length
			};

		}
	}
}
