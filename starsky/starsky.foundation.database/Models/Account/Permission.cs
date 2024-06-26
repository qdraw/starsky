// Copyright © 2017 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace starsky.foundation.database.Models.Account
{
	public class Permission
	{
		public int Id { get; set; }
		public string? Code { get; set; }
		public string? Name { get; set; }
		public int? Position { get; set; }
	}
}
