using System;

namespace starsky.foundation.storage.Storage
{
	public enum ThumbnailSize
	{
		/// <summary>
		/// 150px
		/// </summary>
		TinyMeta,
		
		/// <summary>
		/// 300px
		/// </summary>
		Small, 
		
		/// <summary>
		/// 1000px
		/// </summary>
		Large,
		
		/// <summary>
		/// 2000px
		/// </summary>
		ExtraLarge
	}
	
	public static class ThumbnailNameHelper
	{
		public static int GetSize(ThumbnailSize size)
		{
			switch (size)
			{
				case ThumbnailSize.TinyMeta:
					return 150;
				case ThumbnailSize.Small:
					return 300;
				case ThumbnailSize.Large:
					return 1000;
				case ThumbnailSize.ExtraLarge:
					return 2000;
				default:
					throw new ArgumentOutOfRangeException(nameof(size), size, null);
			}
		}
		
		public static ThumbnailSize GetSize(int size)
		{
			switch (size)
			{
				case 150:
					return ThumbnailSize.TinyMeta;
				case 300:
					return ThumbnailSize.Small;
				case 1000:
					return ThumbnailSize.Large ;
				case 2000:
					return ThumbnailSize.ExtraLarge ;
				default:
					throw new ArgumentOutOfRangeException(nameof(size), size, null);
			}
		}

		public static string Combine(string fileHash, int size)
		{
			return Combine(fileHash, GetSize(size));
		}

		public static string Combine(string fileHash, ThumbnailSize size)
		{
			return fileHash + GetAppend(size);
		}

		private static string GetAppend(ThumbnailSize size)
		{
			switch (size)
			{
				case ThumbnailSize.TinyMeta:
					return "@meta";
				case ThumbnailSize.Small:
					return "@300";
				case ThumbnailSize.Large:
					return string.Empty;
				case ThumbnailSize.ExtraLarge:
					return "@2000";
				default:
					throw new ArgumentOutOfRangeException(nameof(size), size, null);
			}
		}
	}
}
