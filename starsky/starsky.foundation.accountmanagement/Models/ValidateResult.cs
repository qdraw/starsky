using starsky.foundation.accountmanagement.Interfaces;
using starsky.foundation.database.Models.Account;

namespace starsky.foundation.accountmanagement.Models
{
	public class ValidateResult
	{
		public User User { get; set; }
		public bool Success { get; set; }
		public ValidateResultError? Error { get; set; }
        
		public ValidateResult(User user = null, bool success = false, ValidateResultError? error = null)
		{
			User = user;
			Success = success;
			Error = error;
		}
	}
}
