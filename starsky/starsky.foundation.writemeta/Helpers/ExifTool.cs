using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
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
	public class ExifTool : IExifTool
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
		/// <param name="command">exifTool command line args</param>
		/// <returns>true=success</returns>
		public async Task<KeyValuePair<bool, string>> WriteTagsAndRenameThumbnailAsync(string subPath, string command)
		{
			var inputStream = _iStorage.ReadStream(subPath);
			var oldFileHashCodeKeyPair = (await new FileHash(_iStorage).GetHashCodeAsync(subPath));
			
			var runner = new StreamToStreamRunner(_appSettings, inputStream,_logger);
			var stream = await runner.RunProcessAsync(command);

			var newHashCode =
				await RenameThumbnailByStream(oldFileHashCodeKeyPair, stream);
			
			// Set stream to begin for use afterwards
			stream.Seek(0, SeekOrigin.Begin);

			// Need to Dispose for Windows
			inputStream.Close();
			return new KeyValuePair<bool, string>(await _iStorage.WriteStreamAsync(stream, subPath), newHashCode);
		}
		
		internal async Task<string> RenameThumbnailByStream(
			KeyValuePair<string, bool> oldFileHashCodeKeyPair, Stream stream)
		{
			if ( !oldFileHashCodeKeyPair.Value ) return string.Empty;
			byte[] buffer = new byte[FileHash.MaxReadSize];
			await stream.ReadAsync(buffer, 0, FileHash.MaxReadSize);
			
			var newHashCode = await FileHash.CalculateHashAsync(new MemoryStream(buffer));
			if ( string.IsNullOrEmpty(newHashCode)) return string.Empty;

			if ( oldFileHashCodeKeyPair.Key == newHashCode ) return newHashCode;
			
			new ThumbnailFileMoveAllSizes(_thumbnailStorage).FileMove(
				oldFileHashCodeKeyPair.Key, newHashCode);
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
