namespace starsky.foundation.accountmanagement.Interfaces;

public interface ITenantSlugValidator
{
	bool IsValid(string? slug);
}
