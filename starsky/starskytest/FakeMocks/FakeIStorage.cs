using System;
using System.Collections.Generic;
using System.Linq;
using starskycore.Interfaces;
using starskycore.Models;
using starskytest.FakeCreateAn;

namespace starskytest.FakeMocks
{
	class FakeIStorage : IStorage
	{
		private List<string> _outputSubPathFolders = new List<string>();
		private List<string> _outputSubPathFiles  = new List<string>();


		public FakeIStorage(List<string> outputSubPathFolders = null, List<string> outputSubPathFiles = null)
		{
			if ( outputSubPathFolders != null )
			{
				_outputSubPathFolders = outputSubPathFolders;
			}

			if ( outputSubPathFiles != null )
			{
				_outputSubPathFiles = outputSubPathFiles;
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
			var indexOfFolders = _outputSubPathFiles.IndexOf(inputSubPath);
			if ( indexOfFolders == -1 )
			{
				throw new ArgumentException("indexOfFolders---1");
			}
			_outputSubPathFiles[indexOfFolders] = toSubPath;
		}

		public void FileMove(string inputSubPath, string toSubPath)
		{
			var indexOfFiles = _outputSubPathFiles.IndexOf(inputSubPath);
			if ( indexOfFiles == -1 )
			{
				throw new ArgumentException("indexOfFiles---1");
			}
			_outputSubPathFiles[indexOfFiles] = toSubPath;
		}

		public void CreateDirectory(string subPath)
		{
			_outputSubPathFolders.Add(subPath);
		}

		public IEnumerable<string> GetAllFilesInDirectory(string subPath)
		{
			if ( !ExistFolder(subPath) )
			{
				return new List<string>();
			}

			// now GetDirectoryRecursive
			return _outputSubPathFiles.Where(p => p.StartsWith(subPath));

		}

		public IEnumerable<string> GetDirectoryRecursive(string subPath)
		{
			throw new System.NotImplementedException();
		}
	}
}
