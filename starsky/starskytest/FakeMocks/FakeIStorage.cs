using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
		private List<string> _fileHashPerThumbnail  = new List<string>();

		private readonly  Dictionary<string, byte[]> _byteList = new Dictionary<string, byte[]>();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="outputSubPathFolders">/</param>
		/// <param name="outputSubPathFiles">/test.jpg</param>
		/// <param name="byteListSource"></param>
		/// <param name="fileHashPerThumbnail">for mock fileHash=subPath</param>
		public FakeIStorage(List<string> outputSubPathFolders = null, List<string> outputSubPathFiles = null, 
			IReadOnlyList<byte[]> byteListSource = null, List<string> fileHashPerThumbnail = null)
		{
	
			if ( outputSubPathFolders != null )
			{
				_outputSubPathFolders = outputSubPathFolders;
			}

			if ( outputSubPathFiles != null )
			{
				_outputSubPathFiles = outputSubPathFiles;
			}

			if ( fileHashPerThumbnail != null &&  fileHashPerThumbnail.Count == _outputSubPathFiles.Count)
			{
				_fileHashPerThumbnail = fileHashPerThumbnail;
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
			throw new NotImplementedException();
		}

		/// <summary>
		/// Check if exist
		/// </summary>
		/// <param name="fileHash">for mock fileHash</param>
		/// <returns></returns>
		public bool ThumbnailExist(string fileHash)
		{
			if ( _fileHashPerThumbnail.Count == 0 )
				throw new ArgumentException("fill the thumbnail field first");
			return _fileHashPerThumbnail.Any(p => p == fileHash);
		}

		public Stream ThumbnailRead(string fileHash)
		{
			throw new NotImplementedException();
		}

		public bool ThumbnailWriteStream(Stream stream, string fileHash)
		{
			stream.Dispose();
			if ( _fileHashPerThumbnail.Count == 0 )
				throw new ArgumentException("fill the thumbnail bool field first");
			var index = _outputSubPathFiles.IndexOf(fileHash);
			return true;
		}

		public void ThumbnailMove(string fromFileHash, string toFileHash)
		{
			throw new NotImplementedException();
		}

		public bool ThumbnailDelete(string fileHash)
		{
			throw new NotImplementedException();
		}
	}
}
