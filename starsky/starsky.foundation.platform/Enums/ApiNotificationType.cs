namespace starsky.foundation.platform.Enums
{
	public enum ApiNotificationType
	{
		/// <summary>
		/// Default uses anything as payload
		/// </summary>
		Unknown,
		
		/// <summary>
		/// Uses HeartbeatModel as payload
		/// </summary>
		Heartbeat,
		
		/// <summary>
		/// Uses List&lt;FileIndexItem&gt; as payload
		/// </summary>
		ThumbnailGeneration,
		/// <summary>
		/// Uses List&lt;FileIndexItem&gt; as payload
		/// </summary>
		ManualBackgroundSync,
		
		/// <summary>
		/// Uses List&lt;FileIndexItem&gt; as payload
		/// </summary>
		SyncWatcherConnector,
		
		/// <summary>
		/// Uses List&lt;FileIndexItem&gt; as payload
		/// </summary>
		Mkdir,
		
		/// <summary>
		/// Uses List&lt;FileIndexItem&gt; as payload
		/// </summary>
		Rename,
		
		/// <summary>
		/// Uses List&lt;FileIndexItem&gt; as payload
		/// </summary>
		UploadFile,
		
		/// <summary>
		/// Uses List&lt;FileIndexItem&gt; as payload
		/// </summary>
		MetaUpdate,
		
		/// <summary>
		/// Uses List&lt;FileIndexItem&gt; as payload
		/// </summary>
		Replace
	}
}


