using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Storage;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.storage.Storage;

[TestClass]
public sealed class StorageThumbnailFilesystemTest
{
	private readonly string _fileName;
	private readonly StorageThumbnailFilesystem _thumbnailStorage;

	public StorageThumbnailFilesystemTest()
	{
		var createNewImage = new CreateAnImage();
		var appSettings = new AppSettings { ThumbnailTempFolder = createNewImage.BasePath };
		_thumbnailStorage = new StorageThumbnailFilesystem(appSettings, new FakeIWebLogger());
		_fileName = createNewImage.FileName;
	}

	[TestMethod]
	public void CombinePathShouldEndWithTestJpg()
	{
		var result = _thumbnailStorage.CombinePath("test.jpg");
		Assert.IsTrue(result.EndsWith("test.jpg"));
	}

	[TestMethod]
	public void CombinePathShouldEndWithTestJpg2()
	{
		var result = _thumbnailStorage.CombinePath("test.jpg");
		Assert.IsTrue(result.EndsWith("test.jpg"));
	}

	[TestMethod]
	public void FileMove_Test()
	{
		var createNewImage = new CreateAnImage();

		// first copy for parallel test
		_thumbnailStorage.FileCopy(_fileName, "start_move_file");

		_thumbnailStorage.FileMove("start_move_file",
			"StorageThumbnailFilesystemTest_FileMove.jpg");

		var path = Path.Combine(createNewImage.BasePath, "start_move_file" + ".jpg");
		Assert.IsFalse(File.Exists(path));
		var path2 = Path.Combine(createNewImage.BasePath,
			"StorageThumbnailFilesystemTest_FileMove.jpg");
		Assert.IsTrue(File.Exists(path2));

		File.Delete(Path.Combine(createNewImage.BasePath, "start_move_file.jpg"));
		File.Delete(Path.Combine(createNewImage.BasePath,
			"StorageThumbnailFilesystemTest_FileMove.jpg"));

		var createAnImage = new CreateAnImage();
		Assert.IsNotNull(createAnImage);
	}

	[TestMethod]
	public void FileCopy_success()
	{
		var createNewImage = new CreateAnImage();

		_thumbnailStorage.FileCopy(_fileName,
			"StorageThumbnailFilesystemTest_FileCopy.jpg");

		var path = Path.Combine(createNewImage.BasePath, _fileName);
		Assert.IsTrue(File.Exists(path));
		var path2 = Path.Combine(createNewImage.BasePath,
			"StorageThumbnailFilesystemTest_FileCopy.jpg");
		Assert.IsTrue(File.Exists(path2));

		File.Delete(_fileName);
		File.Delete(Path.Combine(createNewImage.BasePath,
			"StorageThumbnailFilesystemTest_FileCopy.jpg"));

		var createAnImage = new CreateAnImage();
		Assert.IsNotNull(createAnImage);
	}

	[TestMethod]
	public void FileCopy_source_notFound()
	{
		var createNewImage = new CreateAnImage();

		_thumbnailStorage.FileCopy("not_found", "StorageThumbnailFilesystemTest_FileCopy2.jpg");

		var path2 = Path.Combine(createNewImage.BasePath,
			"StorageThumbnailFilesystemTest_FileCopy2.jpg");
		Assert.IsFalse(File.Exists(path2));
	}

	[TestMethod]
	public void FileDelete_NotExist()
	{
		Assert.IsFalse(_thumbnailStorage.FileDelete("NotFound"));
	}

	[TestMethod]
	public void ReadStream()
	{
		var createAnImage = new CreateAnImage();
		Assert.IsNotNull(createAnImage);

		var stream = _thumbnailStorage.ReadStream(_fileName);
		Assert.AreEqual(CreateAnImage.Bytes.Length, stream.Length);

		stream.Dispose();
	}

	[TestMethod]
	public void ReadStream_MaxLength()
	{
		var createAnImage = new CreateAnImage();
		Assert.IsNotNull(createAnImage);

		var stream = _thumbnailStorage.ReadStream(_fileName, 100);
		Assert.AreEqual(100, stream.Length);

		stream.Dispose();
	}

	[TestMethod]
	public void WriteStream()
	{
		var createNewImage = new CreateAnImage();

		_thumbnailStorage.WriteStream(new MemoryStream(CreateAnImage.Bytes.ToArray()),
			"StorageThumbnailFilesystemTest_WriteStream");

		var readStream =
			_thumbnailStorage.ReadStream("StorageThumbnailFilesystemTest_WriteStream");
		Assert.AreEqual(CreateAnImage.Bytes.Length, readStream.Length);
		readStream.Dispose();

		File.Delete(Path.Combine(createNewImage.BasePath,
			"StorageThumbnailFilesystemTest_WriteStream.jpg"));
	}

	[TestMethod]
	public async Task WriteStreamAsync()
	{
		var createNewImage = new CreateAnImage();

		await _thumbnailStorage.WriteStreamAsync(
			new MemoryStream(CreateAnImage.Bytes.ToArray()),
			"StorageThumbnailFilesystemTest_WriteStreamAsync");

		var readStream =
			_thumbnailStorage.ReadStream("StorageThumbnailFilesystemTest_WriteStreamAsync");
		Assert.AreEqual(CreateAnImage.Bytes.Length, readStream.Length);
		await readStream.DisposeAsync();

		File.Delete(Path.Combine(createNewImage.BasePath,
			"StorageThumbnailFilesystemTest_WriteStreamAsync.jpg"));
	}

	[TestMethod]
	public void IsFileReady_thumbnailStorage()
	{
		var createNewImage = new CreateAnImage();

		const string thumbnailId = "IsFileReady_thumbnailStorage";
		// first copy for parallel test
		_thumbnailStorage.FileCopy(_fileName, thumbnailId);

		var stream = _thumbnailStorage.ReadStream(thumbnailId);

		var result = _thumbnailStorage.IsFileReady(thumbnailId);
		Assert.IsFalse(result);

		// is disposed to late (as designed)
		stream.Dispose();

		var result2 = _thumbnailStorage.IsFileReady(thumbnailId);
		Assert.IsTrue(result2);

		File.Delete(Path.Combine(createNewImage.BasePath,
			$"{thumbnailId}"));

		Assert.IsFalse(_thumbnailStorage.ExistFile(thumbnailId));
	}
}
