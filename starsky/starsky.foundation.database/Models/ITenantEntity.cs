namespace starsky.foundation.database.Models;

/// <summary>
///     Marker: rows of this entity belong to one tenant. The tenant is a shadow property
///     (TenantPropertyName), not a CLR property: these models are serialized to API clients and
///     copied by reflection helpers, so a TenantId on the class could be set from a request body.
///     ApplicationDbContext adds the column, the query filter and the stamping
/// </summary>
public interface ITenantEntity
{
	public const string TenantPropertyName = "TenantId";
}
