namespace starsky.foundation.storage.Models;

public class HashMp4Result(string fileHash, bool isSuccess, string message)
{
	public string FileHash { get; } = fileHash;
	public bool IsSuccess { get; } = isSuccess;
	public string Message { get; } = message;
}
