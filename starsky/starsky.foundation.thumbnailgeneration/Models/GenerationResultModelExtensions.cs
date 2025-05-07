using System.Collections.Generic;
using System.Linq;

namespace starsky.foundation.thumbnailgeneration.Models;

public static class GenerationResultModelExtensions
{
	public static List<GenerationResultModel> AddOrUpdateRange(
		this IEnumerable<GenerationResultModel>? compositeResults,
		IEnumerable<GenerationResultModel>? result)
	{
		return AddOrUpdateRange(compositeResults?.ToList(), result?.ToList());
	}

	public static List<GenerationResultModel> AddOrUpdateRange(
		this List<GenerationResultModel>? compositeResults,
		List<GenerationResultModel>? result)
	{
		if ( compositeResults == null && result != null )
		{
			return result;
		}

		if ( result == null && compositeResults != null )
		{
			return compositeResults;
		}

		if ( compositeResults == null )
		{
			return [];
		}

		foreach ( var resultItem in result! )
		{
			var existingItem = compositeResults.FirstOrDefault(p =>
				p.FileHash == resultItem.FileHash && p.Size == resultItem.Size);
			if ( existingItem != null )
			{
				existingItem.SubPath = resultItem.SubPath;
				existingItem.Success = resultItem.Success;
				existingItem.IsNotFound = resultItem.IsNotFound;
				existingItem.ErrorMessage = resultItem.ErrorMessage;
				existingItem.Size = resultItem.Size;
				existingItem.ImageFormat = resultItem.ImageFormat;
				existingItem.ToGenerate = resultItem.ToGenerate;
			}
			else
			{
				compositeResults.Add(resultItem);
			}
		}

		return compositeResults;
	}
}
