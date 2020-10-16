using System;
using System.Collections.Generic;
using starsky.foundation.database.Models.Account;

namespace starsky.foundation.accountmanagement.Models.Account
{
	public class UserIdentifierStatusModel
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public DateTime Created { get; set; }
		public List<string> CredentialsIdentifiers { get; set; } = new List<string>();
		public List<int> CredentialTypeIds { get; set; } = new List<int>();
	}
}
