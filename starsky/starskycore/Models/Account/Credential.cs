// Copyright © 2017 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace starskycore.Models.Account
{
    public class Credential
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CredentialTypeId { get; set; }
        public string Identifier { get; set; } // email
        public string Secret { get; set; }
        public string Extra { get; set; }
        public User User { get; set; }
        public CredentialType CredentialType { get; set; }
    }
}