#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.writemeta.Interfaces;
using static Medallion.Shell.Shell;

namespace starsky.foundation.writemeta.Helpers
{
	/// <summary>
	/// Only for writing commands 
	/// Check for mapping objects to exifTool commandline args -> 'ExifToolCmdHelper'
	/// </summary>
	public sealed class ExifTool : IExifTool
	{
		private readonly AppSettings _appSettings;
		private readonly IStorage _iStorage;
		private readonly IStorage _thumbnailStorage;
		private readonly IWebLogger _logger;

		public ExifTool(IStorage sourceStorage, IStorage thumbnailStorage, AppSettings appSettings, IWebLogger logger)
		{
			_appSettings = appSettings;
			_iStorage = sourceStorage;
			_thumbnailStorage = thumbnailStorage;
			_logger = logger;
		}

		/// <summary>
		/// Write commands to ExifTool for ReadStream
		/// </summary>
		/// <param name="subPath">the location</param>
		/// <param name="beforeFileHash">thumbnail fileHash</param>
		/// <param name="command">exifTool command line args</param>
		/// <param name="cancellationToken">to cancel</param>
		/// <returns>true=success, newFileHash</returns>
		[SuppressMessage("ReSharper", "InvertIf")]
		public async Task<KeyValuePair<bool, string>> WriteTagsAndRenameThumbnailAsync(string subPath, 
			string? beforeFileHash, string command, CancellationToken cancellationToken = default)
		{
			var inputStream = _iStorage.ReadStream(subPath);
			beforeFileHash ??= ( await new FileHash(_iStorage).GetHashCodeAsync(subPath) ).Key;
			
			var runner = new StreamToStreamRunner(_appSettings, inputStream,_logger);
			var stream = await runner.RunProcessAsync(command);

			var newHashCode = await RenameThumbnailByStream(beforeFileHash, stream,
				!beforeFileHash.Contains(FileHash.GeneratedPostFix), cancellationToken);
			
			// Set stream to begin for use afterwards
			stream.Seek(0, SeekOrigin.Begin);
			
			// Need to Dispose for Windows
			inputStream.Close();

			if ( stream.Length <= 15 && (await PlainTextFileHelper.StreamToStringAsync(stream))
			    .Contains("Fake ExifTool", StringComparison.InvariantCultureIgnoreCase))
			{
				_logger.LogError($"[WriteTagsAndRenameThumbnailAsync] Fake Exiftool detected {subPath}");
				return new KeyValuePair<bool, string>(false, beforeFileHash);
			}
			
			return new KeyValuePair<bool, string>(await _iStorage.WriteStreamAsync(stream, subPath), newHashCode);
		}
		
		[SuppressMessage("ReSharper", "MustUseReturnValue")]
		internal async Task<string> RenameThumbnailByStream(
			string beforeFileHash, Stream stream, bool isSuccess, CancellationToken cancellationToken = default)
		{
			if ( string.IsNullOrEmpty(beforeFileHash) || !isSuccess ) return string.Empty;
			var buffer = new byte[FileHash.MaxReadSize];
			await stream.ReadAsync(buffer.AsMemory(0, FileHash.MaxReadSize), cancellationToken);
			
			var newHashCode = await FileHash.CalculateHashAsync(new MemoryStream(buffer), cancellationToken);
			if ( string.IsNullOrEmpty(newHashCode)) return string.Empty;

			if ( beforeFileHash == newHashCode ) return newHashCode;
			
			new ThumbnailFileMoveAllSizes(_thumbnailStorage).FileMove(
				beforeFileHash, newHashCode);
			return newHashCode;
		}

		/// <summary>
		/// Write commands to ExifTool for ReadStream
		/// </summary>
		/// <param name="subPath">the location</param>
		/// <param name="command">exifTool command line args</param>
		/// <returns>true=success</returns>
		public async Task<bool> WriteTagsAsync(string subPath, string command)
		{
			var inputStream = _iStorage.ReadStream(subPath);
		
			var runner = new StreamToStreamRunner(_appSettings, inputStream,_logger);
			var stream = await runner.RunProcessAsync(command);

			// Need to Dispose for Windows
			inputStream.Close();
			return await _iStorage.WriteStreamAsync(stream, subPath);
		}

		/// <summary>
		/// Write commands to ExifTool for ThumbnailWriteStream (Does NOT work with mono/legacy)
		/// </summary>
		/// <param name="fileHash">the location</param>
		/// <param name="command">exifTool command line args</param>
		/// <returns>true=success</returns>
		public async Task<bool> WriteTagsThumbnailAsync(string fileHash, string command)
		{
			var inputStream = _thumbnailStorage.ReadStream(fileHash);
			var runner = new StreamToStreamRunner(_appSettings, inputStream,_logger);
			var stream = await runner.RunProcessAsync(command);
			// Need to Dispose for Windows
			inputStream.Close();
			return await _thumbnailStorage.WriteStreamAsync(stream, fileHash);
		}

	}
	
	/// <summary>
	/// Handle ExifTool Streaming
	/// </summary>
	internal class StreamToStreamRunner
	{
		private readonly Stream _src;
		private readonly AppSettings _appSettings;
		private readonly IWebLogger _logger;

		public StreamToStreamRunner(AppSettings appSettings, Stream src, IWebLogger logger)
		{
			_src = src ?? throw new ArgumentNullException(nameof(src));
			_appSettings = appSettings;
			_logger = logger;
		}


		/// <summary>
		/// Run Command async
		/// </summary>
		/// <param name="optionsArgs">exifTool args</param>
		/// <returns>bool if success</returns>
		/// <exception cref="ArgumentException">if exifTool is missing</exception>
		public async Task<Stream> RunProcessAsync(string optionsArgs)
		{
			var args = $"{optionsArgs} -o - -";

			var ms = new MemoryStream();

			try
			{
					
				// run with pipes
				var cmd =  Default.Run(_appSettings.ExifToolPath, options: opts => {
					opts.StartInfo(si => si.Arguments = args);
				}) < _src > ms;
					
				var result = await cmd.Task.ConfigureAwait(false);

				// option without pipes:
				//	await cmd.StandardInput.PipeFromAsync(_src).ConfigureAwait(false) await
				// cmd.StandardOutput.BaseStream.CopyToAsync(ms).ConfigureAwait(false)
					
				if ( _appSettings.IsVerbose() ) _logger.LogInformation($"[RunProcessAsync] ~ exifTool {optionsArgs} " +
					$"run with result: {result.Success} ~ ");
	
				ms.Seek(0, SeekOrigin.Begin);

				return ms;
			}
			catch (Win32Exception ex)
			{
				throw new ArgumentException("Error when trying to start the exifTool process.  " +
				                            "Please make sure exifTool is installed, and its path is properly " +
				                            "specified in the options.", ex);
			}
		}
			
	}
}
