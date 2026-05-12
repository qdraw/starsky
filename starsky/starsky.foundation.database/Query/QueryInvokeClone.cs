using starsky.foundation.database.Data;
using starsky.foundation.database.Interfaces;

namespace starsky.foundation.database.Query;

public partial class Query // For invoke & clone only
{
	public void Invoke(ApplicationDbContext applicationDbContext)
	{
		_context = applicationDbContext;
	}

	public IQuery Clone(ApplicationDbContext applicationDbContext)
	{
		var query = ( IQuery ) MemberwiseClone();
		query.Invoke(applicationDbContext);
		return query;
	}
}
