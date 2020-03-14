using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;
using starskycore.Services;
using starskytest.FakeCreateAn;
using starskytest.Services;

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
		
		public bool ExistFile(string subPath)
		{
			return _outputSubPathFiles.Contains(PathHelper.PrefixDbSlash(subPath));
		}

		public bool ExistFolder(string subPath)
		{
			return _outputSubPathFolders.Contains(PathHelper.PrefixDbSlash(subPath));
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
			throw new NotImplementedException();
		}

		public bool FileDelete(string path)
		{
			if ( !ExistFile(path) ) return false;
			var index = _outputSubPathFiles.IndexOf(path);
			_outputSubPathFiles[index] = null;
			return true;
		}

		public void CreateDirectory(string subPath)
		{
			_outputSubPathFolders.Add(subPath);
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

		public IEnumerable<string> GetAllFilesInDirectoryRecursive(string fullFilePath)
		{
			throw new NotImplementedException();
		}

		private bool CheckAndFixParentFiles(string parentFolder, string filePath)
		{
			if ( parentFolder != string.Empty && !filePath.StartsWith(parentFolder) ) return false;

			var value = $"^{Regex.Escape(parentFolder)}" + "\\/\\w+.[a-z]{3}$";
			
			return Regex.Match(filePath, $"^{Regex.Escape(parentFolder)}"+ "\\/\\w+.[a-z]{3}$").Success;
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
			if ( !ExistFile(path) ) throw new FileNotFoundException(path);

			var result = _byteList.FirstOrDefault(p => p.Key == path).Value;
			MemoryStream stream1 = new MemoryStream(result);
			return stream1;
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

	}
}
