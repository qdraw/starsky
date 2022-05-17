using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using starsky.foundation.database.Models;

namespace starsky.foundation.database.Query
{
	public static class SolveConcurrency
	{
		internal static void SolveConcurrencyExceptionLoop(
			IReadOnlyList<EntityEntry> concurrencyExceptionEntries)
		{
			foreach (var entry in concurrencyExceptionEntries)
			{
				SolveConcurrencyException(entry.Entity, entry.CurrentValues,
					entry.GetDatabaseValues(), entry.Metadata.Name, 
					// former values from database
					entry.CurrentValues.SetValues);
			}
		}
		
		/// <summary>
		/// Delegate to abstract OriginalValues Setter
		/// </summary>
		/// <param name="propertyValues"> propertyValues</param>
		internal delegate void OriginalValuesSetValuesDelegate(PropertyValues propertyValues);

		/// <summary>
		/// Database concurrency refers to situations in which multiple processes or users access or change the same data in a database at the same time.
		/// @see: https://docs.microsoft.com/en-us/ef/core/saving/concurrency
		/// </summary>
		/// <param name="entryEntity">item</param>
		/// <param name="proposedValues">new update</param>
		/// <param name="databaseValues">old database item</param>
		/// <param name="entryMetadataName">meta name</param>
		/// <param name="entryOriginalValuesSetValues">entry item</param>
		/// <exception cref="NotSupportedException">unknown how to fix</exception>
		internal static void SolveConcurrencyException(object entryEntity, 
			PropertyValues proposedValues, PropertyValues databaseValues, string entryMetadataName, 
			OriginalValuesSetValuesDelegate entryOriginalValuesSetValues)
		{
			if ( !( entryEntity is FileIndexItem ) )
				throw new NotSupportedException(
					"Don't know how to handle concurrency conflicts for "
					+ entryMetadataName);
	        
			foreach (var property in proposedValues.Properties)
			{
				var proposedValue = proposedValues[property];
				proposedValues[property] = proposedValue;
			}

			// Refresh original values to bypass next concurrency check
			if ( databaseValues != null )
			{
				entryOriginalValuesSetValues(databaseValues);
			}
		}
	}
	
}

