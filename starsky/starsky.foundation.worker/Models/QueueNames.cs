namespace starsky.foundation.worker.Models;

/// <summary>
///     Queue names used for settings keys, database QueueName discriminator and RabbitMQ queue names.
/// </summary>
public static class QueueNames
{
	public const string Update = "Update";
	public const string Thumbnail = "Thumbnail";
	public const string DiskWatcher = "DiskWatcher";
	public const string ImageClassification = "ImageClassification";
}

