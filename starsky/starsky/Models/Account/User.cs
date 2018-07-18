// Copyright © 2017 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace starsky.Models.Account
{
public class User
{
public int Id { get; set; }
public string Name { get; set; }
public DateTime Created { get; set; }

public virtual ICollection<Credential> Credentials { get; set; }
}
}