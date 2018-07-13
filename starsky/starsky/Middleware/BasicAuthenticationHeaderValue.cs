using System;

namespace starsky.Middleware
{
    public class BasicAuthenticationHeaderValue
    {
        public BasicAuthenticationHeaderValue(string authenticationHeaderValue)
        {
            if (!string.IsNullOrWhiteSpace(authenticationHeaderValue))
            {
                _authenticationHeaderValue = authenticationHeaderValue;
                if (TryDecodeHeaderValue())
                {
                    ReadAuthenticationHeaderValue();
                }
            }
        }

        private readonly string _authenticationHeaderValue;
        private string[] _splitDecodedCredentials;

        public bool IsValidBasicAuthenticationHeaderValue { get; private set; }
        public string UserIdentifier { get; private set; }
        public string UserPassword { get; private set; }

        private bool TryDecodeHeaderValue()
        {
            const int headerSchemeLength = 6; 
            // The Length of the word "Basic "
            if (_authenticationHeaderValue.Length <= headerSchemeLength)
            {
                return false;
            }
            var encodedCredentials = _authenticationHeaderValue.Substring(headerSchemeLength);
            try
            {
                var decodedCredentials = Convert.FromBase64String(encodedCredentials);
                _splitDecodedCredentials = System.Text.Encoding.ASCII.GetString(decodedCredentials).Split(':');
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private void ReadAuthenticationHeaderValue()
        {
            IsValidBasicAuthenticationHeaderValue = _splitDecodedCredentials.Length == 2
                                                   && !string.IsNullOrWhiteSpace(_splitDecodedCredentials[0])
                                                   && !string.IsNullOrWhiteSpace(_splitDecodedCredentials[1]);
            if (IsValidBasicAuthenticationHeaderValue)
            {
                UserIdentifier = _splitDecodedCredentials[0];
                UserPassword = _splitDecodedCredentials[1];
            }
        }
    }
}