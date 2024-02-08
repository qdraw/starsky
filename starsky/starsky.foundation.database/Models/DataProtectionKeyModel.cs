using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Warning for cref=EntityFrameworkCoreXmlRepository
#pragma warning disable CS1574, CS1584, CS1581, CS1580

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
namespace starsky.foundation.database.Models;

/// <summary>
/// Code first model used by <see cref="EntityFrameworkCoreXmlRepository{TContext}"/>.
/// </summary>
public class DataProtectionKey
{
	/// <summary>
	/// The entity identifier of the <see cref="DataProtectionKey"/>.
	/// </summary>
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }

	/// <summary>
	/// The friendly name of the <see cref="DataProtectionKey"/>.
	/// </summary>
	[MaxLength(100)]
	public string? FriendlyName { get; set; }

	/// <summary>
	/// The XML representation of the <see cref="DataProtectionKey"/>.
	/// </summary>
	[MaxLength(400)]
	public string? Xml { get; set; }
}
