using System.Collections.Generic;
using starsky.foundation.database.Models;

namespace starsky.feature.webhtmlpublish.Models;

public sealed class PublishCreateBackgroundJobPayload
{
	public List<FileIndexItem> Info { get; set; } = [];
	public string PublishProfileName { get; set; } = string.Empty;
	public string ItemName { get; set; } = string.Empty;
	public string Location { get; set; } = string.Empty;
}

