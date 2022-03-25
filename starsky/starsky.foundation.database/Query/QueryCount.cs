#nullable enable
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using starsky.foundation.database.Models;

namespace starsky.foundation.database.Query
{
	/// <summary>
	/// QueryCount
	/// </summary>
	public partial class Query
	{
		public Task<int> CountAsync(Expression<Func<FileIndexItem, bool>>? expression = null)
		{
			return expression == null ? _context.FileIndex.CountAsync() : _context.FileIndex.CountAsync(expression);
		}
	}
	
}

