using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using starskycore.Interfaces;
using starskycore.Models;
using static Medallion.Shell.Shell;

namespace starskycore.Services
{
	public class ExifTool : IExifTool
	{
		private static AppSettings _appSettings;
		private readonly IStorage _iStorage;

		public ExifTool(IStorage iStorage, AppSettings appSettings)
		{
			_appSettings = appSettings;
			_iStorage = iStorage;
		}
		
		public async Task<bool> WriteTagsAsync(string subPath, string command)
		{
			var runner = new StreamToStreamRunner(_appSettings, _iStorage.ReadStream(subPath));
			var stream = await runner.RunProcessAsync(command);
			return _iStorage.WriteStream(stream, subPath);
		}

		public async Task<bool> WriteTagsThumbnailAsync(string fileHash, string command)
		{
			var runner = new StreamToStreamRunner(_appSettings, _iStorage.ThumbnailRead(fileHash));
			var stream = await runner.RunProcessAsync(command);
			return _iStorage.ThumbnailWriteStream(stream, fileHash);
		}

		private class StreamToStreamRunner
		{
			private readonly Stream _src;


			public StreamToStreamRunner(AppSettings appSettings, Stream src)
			{
				_src = src ?? throw new ArgumentNullException(nameof(src));
				_appSettings = appSettings;
			}


			public async Task<Stream> RunProcessAsync(string optionsArgs)
			{
				var args = $"{optionsArgs} -o - -";

				var ms = new MemoryStream();

				try
				{
					var cmd =  Default.Run(_appSettings.ExifToolPath, options: opts => {
						opts.StartInfo(si => si.Arguments = args);
					}) < _src > ms;

					var result = await cmd.Task.ConfigureAwait(false);

					Console.WriteLine(result.Success);

					ms.Seek(0, SeekOrigin.Begin);

					return ms;
				}
				catch (Win32Exception ex)
				{
					throw new Exception("Error when trying to start the exiftool process.  " +
					                    "Please make sure exiftool is installed, and its path is properly specified in the options.", ex);
				}
			}
			
		}
		
	}
}
