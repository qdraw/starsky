using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;
using starskytest.FakeCreateAn;
using starskytest.Services;

namespace starskytest.FakeMocks
{
	class FakeIStorage : IStorage
	{
		private List<string> _outputSubPathFolders = new List<string>();
		private List<string> _outputSubPathFiles  = new List<string>();

		private readonly byte[] _byteArray;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="outputSubPathFolders">/</param>
		/// <param name="outputSubPathFiles">/test.jpg</param>
		/// <param name="byteArray"></param>
		public FakeIStorage(List<string> outputSubPathFolders = null, List<string> outputSubPathFiles = null, byte[] byteArray = null)
		{
			if ( outputSubPathFolders != null )
			{
				_outputSubPathFolders = outputSubPathFolders;
			}

			if ( outputSubPathFiles != null )
			{
				_outputSubPathFiles = outputSubPathFiles;
			}

			if ( byteArray != null )
			{
				_byteArray = byteArray;
			}
		}
		
		public bool ExistFile(string subPath)
		{
			return _outputSubPathFiles.Contains(subPath);
		}

		public bool ExistFolder(string subPath)
		{
			return _outputSubPathFolders.Contains(subPath);
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
			throw new NotImplementedException();
		}

		public void CreateDirectory(string subPath)
		{
			_outputSubPathFolders.Add(subPath);
		}

		public IEnumerable<string> GetAllFilesInDirectory(string subPath)
		{
			subPath = PathHelper.RemoveLatestSlash(subPath);
			
			// non recruisive
			if ( !ExistFolder(subPath) )
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
			if ( !filePath.StartsWith(parentFolder) ) return false;
			
			return Regex.Match(filePath, $"^{parentFolder}"+ "\\/\\w+.[a-z]{3}$").Success;
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
			if ( ExistFile(path) && _byteArray == null)
			{
				byte[] byteArray = Encoding.UTF8.GetBytes("test");
				MemoryStream stream = new MemoryStream(byteArray);
				return stream;
			}
			if ( ExistFile(path) )
			{
				MemoryStream stream = new MemoryStream(_byteArray);
				return stream;
			}
			throw new FileNotFoundException(path);
		}

		public bool ExistThumbnail(string fileHash)
		{
			throw new NotImplementedException();
		}

		public Stream Thumbnail(string fileHash)
		{
			throw new NotImplementedException();
		}
	}
}
