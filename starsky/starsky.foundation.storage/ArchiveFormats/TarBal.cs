using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.platform.Helpers;
using starsky.foundation.storage.Interfaces;

namespace starsky.foundation.storage.ArchiveFormats
{
	[SuppressMessage("Usage", "CA1835:Prefer the 'Memory'-based overloads for 'ReadAsync' and 'WriteAsync")]
	public sealed class TarBal
    {
	    private readonly IStorage _storage;
	    public TarBal(IStorage storage)
	    {
		    _storage = storage;
	    }

	    /// <summary>
	    /// Extracts a <i>.tar.gz</i> archive stream to the specified directory.
	    /// </summary>
	    /// <param name="stream">The <i>.tar.gz</i> to decompress and extract.</param>
	    /// <param name="outputDir">Output directory to write the files.</param>
	    /// <param name="cancellationToken">cancellationToken</param>
	    public async Task ExtractTarGz(Stream stream, string outputDir, CancellationToken cancellationToken)
        {
	        // A GZipStream is not seekable, so copy it first to a MemoryStream
	        using var gzip = new GZipStream(stream, CompressionMode.Decompress);
	        const int chunk = 4096;
	        using var memStr = new MemoryStream();
	        int read;
	        var buffer = new byte[chunk];
                    
	        while ((read = await gzip.ReadAsync(buffer, 0, buffer.Length,cancellationToken)) > 0)
	        {
		        await memStr.WriteAsync(buffer, 0, read, cancellationToken);
	        }

	        memStr.Seek(0, SeekOrigin.Begin);
	        await ExtractTar(memStr, outputDir, cancellationToken);
        }

	    /// <summary>
	    /// Extracts a <c>tar</c> archive to the specified directory.
	    /// </summary>
	    /// <param name="stream">The <i>.tar</i> to extract.</param>
	    /// <param name="outputDir">Output directory to write the files.</param>
	    /// <param name="cancellationToken">cancellationToken</param>
	    [SuppressMessage("ReSharper", "MustUseReturnValue")]
	    public async Task ExtractTar(Stream stream, string outputDir,
		    CancellationToken cancellationToken)
        {
            var buffer = new byte[100];
            while (true)
            {
                await stream.ReadAsync(buffer, 0, 100, cancellationToken);
                var name = Encoding.ASCII.GetString(buffer).Trim('\0');
                if ( string.IsNullOrEmpty(name) || stream.Position > stream.Length)
                {
	                break;
                }
                stream.Seek(24, SeekOrigin.Current);
                await stream.ReadAsync(buffer, 0, 12,cancellationToken);
                var size = Convert.ToInt64(Encoding.UTF8.GetString(buffer, 0, 12).Trim('\0').Trim(), 8);

                stream.Seek(376L, SeekOrigin.Current);

                var output = $"{outputDir}/{name}";
                if ( !_storage.ExistFolder(FilenamesHelper.GetParentPath(output)) )
                {
	                _storage.CreateDirectory(FilenamesHelper.GetParentPath(output));
                }
                if(!name.EndsWith("/", StringComparison.InvariantCulture)) 
                {
	                var str = new MemoryStream();
	                var buf = new byte[size];
	                await stream.ReadAsync(buf, 0, buf.Length,cancellationToken);
	                str.Write(buf, 0, buf.Length);
	                _storage.WriteStreamOpenOrCreate(str, output);
                }

                var pos = stream.Position;

                var offset = 512 - (pos % 512);
                if (offset == 512)
                    offset = 0;

                stream.Seek(offset, SeekOrigin.Current);
            }
        }
    }
}
