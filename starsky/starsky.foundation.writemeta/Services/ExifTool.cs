using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.writemeta.Interfaces;
using static Medallion.Shell.Shell;

namespace starsky.foundation.writemeta.Services
{
	/// <summary>
	/// Only for writing commands 
	/// Check for mapping objects to exifTool commandline args -> 'ExifToolCmdHelper'
	/// </summary>
	[Service(typeof(IExifTool), InjectionLifetime = InjectionLifetime.Scoped)]
	public class ExifTool : IExifTool
	{
		private readonly AppSettings _appSettings;
		private readonly IStorage _iStorage;
		private readonly IStorage _thumbnailStorage;

		public ExifTool(ISelectorStorage selectorStorage, AppSettings appSettings)
		{
			_appSettings = appSettings;
			_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			_thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
		}
		
		/// <summary>
		/// Write commands to ExifTool for ReadStream (Does NOT work with mono/legacy)
		/// </summary>
		/// <param name="subPath">the location</param>
		/// <param name="command">exifTool command line args</param>
		/// <returns>true=success</returns>
		public async Task<bool> WriteTagsAsync(string subPath, string command)
		{
			var inputStream = _iStorage.ReadStream(subPath);
			var runner = new StreamToStreamRunner(_appSettings, inputStream);
			var stream = await runner.RunProcessAsync(command);
			inputStream.Dispose();
			Console.Write("‘");
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
			var runner = new StreamToStreamRunner(_appSettings, _thumbnailStorage.ReadStream(fileHash));
			var stream = await runner.RunProcessAsync(command);
			return await _thumbnailStorage.WriteStreamAsync(stream, fileHash);
		}

		/// <summary>
		/// Handle ExifTool Streaming
		/// </summary>
		private class StreamToStreamRunner
		{
			private readonly Stream _src;
			private readonly AppSettings _appSettings;

			public StreamToStreamRunner(AppSettings appSettings, Stream src)
			{
				_src = src ?? throw new ArgumentNullException(nameof(src));
				_appSettings = appSettings;
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
					
					if ( _appSettings.Verbose ) Console.WriteLine($"~ exifTool {optionsArgs} run with result: {result.Success} ~ ");
	
					ms.Seek(0, SeekOrigin.Begin);

					return ms;
				}
				catch (Win32Exception ex)
				{
					throw new ArgumentException("Error when trying to start the exifTool process.  " +
					                            "Please make sure exifTool is installed, and its path is properly specified in the options.", ex);
				}
			}
			
		}
		
	}
}
