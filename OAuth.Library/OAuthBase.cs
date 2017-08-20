using RestSharp.Extensions.MonoHttp;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace OAuth.Library

{
    public sealed class OAuthBase
    {
        /// <summary>
        /// Provides a predefined set of algorithms that are supported officially by the protocol
        /// </summary>
        public enum SignatureTypes
        {
            Hmacsha1,
            Plaintext,
            Rsasha1
        }


        /// <summary>
        /// Provides an internal structure to sort the query parameter
        /// </summary>
        private class QueryParameter
        {
            public QueryParameter(string name, string value)
            {
                Name = name;
                Value = value;
            }

            public string Name { get; }
            public string Value { get; }
        }


        /// <summary>
        /// Comparer class used to perform the sorting of the query parameters
        /// </summary>
        private class QueryParameterComparer : IComparer<QueryParameter>
        {
            #region IComparer<QueryParameter> Members
            public int Compare(QueryParameter x, QueryParameter y)
                => x?.Name == y?.Name
                ? string.CompareOrdinal(x?.Value, y?.Value)
                : string.CompareOrdinal(x?.Name, y?.Name);
            #endregion
        }

        private const string OAuthVersion = "1.0";
        private const string OAuthParameterPrefix = "oauth_";

        //
        // List of know and used oauth parameters' names
        //        
        private const string OAuthConsumerKeyKey = "oauth_consumer_key";
        private const string OAuthVersionKey = "oauth_version";
        private const string OAuthSignatureMethodKey = "oauth_signature_method";
        private const string OAuthTimestampKey = "oauth_timestamp";
        private const string OAuthNonceKey = "oauth_nonce";
        private const string OAuthTokenKey = "oauth_token";
        private const string OAuthVerifier = "oauth_verifier";    // OAuth 1.0a

        private const string Hmacsha1SignatureType = "HMAC-SHA1";

        private const string Status = "status";
        private const string FilterCreatedAtMin = "filter[created_at_min]";

        private readonly Random _random = new Random();
        private readonly string _unreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";



        /// <summary>
        /// Helper function to compute a hash value
        /// </summary>
        /// <param name="hashAlgorithm">The hashing algoirhtm used. If that algorithm needs some initialization, like HMAC and its derivatives, they should be initialized prior to passing it to this function</param>
        /// <param name="data">The data to hash</param>
        /// <returns>a Base64 string of the hash value</returns>
        private string ComputeHash(HashAlgorithm hashAlgorithm, string data)
        {
            if (hashAlgorithm == null)
                throw new ArgumentNullException(nameof(hashAlgorithm));

            if (string.IsNullOrEmpty(data))
                throw new ArgumentNullException(nameof(data));

            byte[] dataBuffer = Encoding.ASCII.GetBytes(data);
            byte[] hashBytes = hashAlgorithm.ComputeHash(dataBuffer);

            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Internal function to cut out all non oauth query string parameters (all parameters not begining with "oauth_")
        /// </summary>
        /// <param name="parameters">The query string part of the Url</param>
        /// <returns>A list of QueryParameter each containing the parameter name and value</returns>
        private static List<QueryParameter> GetQueryParameters(string parameters)
        {
            if (parameters.StartsWith("?"))
            {
                parameters = parameters.Remove(0, 1);
            }

            List<QueryParameter> result = new List<QueryParameter>();

            if (!string.IsNullOrEmpty(parameters))
            {
                string[] p = parameters.Split('&');
                foreach (string s in p)
                {
                    if (!string.IsNullOrEmpty(s) && !s.StartsWith(OAuthParameterPrefix))
                    {
                        if (s.IndexOf('=') > -1)
                        {
                            string[] temp = s.Split('=');
                            result.Add(new QueryParameter(temp[0], temp[1]));
                        }
                        else
                        {
                            result.Add(new QueryParameter(s, string.Empty));
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// This is a different Url Encode implementation since the default .NET one outputs the percent encoding in lower case.
        /// While this is not a problem with the percent encoding spec, it is used in upper case throughout OAuth
        /// </summary>
        /// <param name="value">The value to Url encode</param>
        /// <returns>Returns a Url encoded string</returns>
        private string UrlEncode(string value)
        {
            StringBuilder result = new StringBuilder();

            foreach (char symbol in value)
            {
                if (_unreservedChars.IndexOf(symbol) != -1)
                {
                    result.Append(symbol);
                }
                else
                {
                    result.Append('%' + $"{(int)symbol:X2}");
                }
            }

            return result.ToString();
        }


        /// <summary>
        /// Normalizes the request parameters according to the spec
        /// </summary>
        /// <param name="parameters">The list of parameters already sorted</param>
        /// <returns>a string representing the normalized parameters</returns>
        private static string NormalizeRequestParameters(IList<QueryParameter> parameters)
        {
            StringBuilder sb = new StringBuilder();

            for (var i = 0; i < parameters.Count; i++)
            {
                QueryParameter p = parameters[i];
                sb.AppendFormat("{0}={1}", p.Name, p.Value);

                if (i < parameters.Count - 1)
                {
                    sb.Append("&");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generate the signature base that is used to produce the signature
        /// </summary>
        /// <param name="url">The full url that needs to be signed including its non OAuth url parameters</param>
        /// <param name="consumerKey">The consumer key</param>        
        /// <param name="token">The token, if available. If not available pass null or an empty string</param>
        /// <param name="verifier"></param>
        /// <param name="filterCreatedAtMin">The order's date filterCreatedAtMin, if available. For example - '2017-01-01'. If not available pass null or an empty string</param>
        /// <param name="status">The order's status, if available. If not available pass null or an empty string</param>
        /// <param name="httpMethod">The http method used. Must be a valid HTTP method verb (POST,GET,PUT, etc)</param>
        /// <param name="nonce"></param>
        /// <param name="signatureType">The signature type. To use the default values use <see cref="OAuthBase.SignatureTypes">OAuthBase.SignatureTypes</see>.</param>
        /// <param name="timeStamp"></param>
        /// <param name="normalizedUrl"></param>
        /// <param name="normalizedRequestParameters"></param>
        /// <returns>The signature base</returns>
        private string GenerateSignatureBase(
            Uri url, string consumerKey, string token,
            string httpMethod, string timeStamp, string nonce, string signatureType,
            string verifier, string filterCreatedAtMin, string status,
            out string normalizedUrl, out string normalizedRequestParameters)
        {
            if (token == null)
            {
                token = string.Empty;
            }
            if (string.IsNullOrEmpty(consumerKey))
            {
                throw new ArgumentNullException(nameof(consumerKey));
            }
            if (string.IsNullOrEmpty(httpMethod))
            {
                throw new ArgumentNullException(nameof(httpMethod));
            }
            if (string.IsNullOrEmpty(signatureType))
            {
                throw new ArgumentNullException(nameof(signatureType));
            }

            if (string.IsNullOrEmpty(verifier))
                throw new ArgumentException("message", nameof(verifier));
            List<QueryParameter> parameters = GetQueryParameters(url.Query);
            parameters.Add(new QueryParameter(OAuthVersionKey, OAuthVersion));
            parameters.Add(new QueryParameter(OAuthNonceKey, nonce));
            parameters.Add(new QueryParameter(OAuthTimestampKey, timeStamp));
            parameters.Add(new QueryParameter(OAuthSignatureMethodKey, signatureType));
            parameters.Add(new QueryParameter(OAuthConsumerKeyKey, consumerKey));

            if (!string.IsNullOrEmpty(verifier))
                parameters.Add(new QueryParameter(OAuthVerifier, verifier));

            if (!string.IsNullOrEmpty(status))
                parameters.Add(new QueryParameter(Status, status));

            if (!string.IsNullOrEmpty(filterCreatedAtMin))
                parameters.Add(new QueryParameter(HttpUtility.UrlEncode(FilterCreatedAtMin).Replace("%5b", "%5B").Replace("%5d", "%5D"), filterCreatedAtMin));

            if (!string.IsNullOrEmpty(token))
            {
                parameters.Add(new QueryParameter(OAuthTokenKey, token));
            }

            parameters.Sort(new QueryParameterComparer());
            normalizedUrl = $"{url.Scheme}://{url.Host}";

            if (!((url.Scheme == "http" && url.Port == 80) || (url.Scheme == "https" && url.Port == 443)))
            {
                normalizedUrl += ":" + url.Port;
            }

            normalizedUrl += url.AbsolutePath;
            normalizedRequestParameters = NormalizeRequestParameters(parameters);

            StringBuilder signatureBase = new StringBuilder();
            signatureBase.AppendFormat("{0}&", httpMethod.ToUpper());
            signatureBase.AppendFormat("{0}&", UrlEncode(normalizedUrl));
            signatureBase.AppendFormat("{0}", UrlEncode(normalizedRequestParameters));

            return signatureBase.ToString();
        }

        /// <summary>  
        /// Generate the signature value based on the given signature base and hash algorithm
        /// </summary>
        /// <param name="signatureBase">The signature based as produced by the GenerateSignatureBase method or by any other means</param>
        /// <param name="hash">The hash algorithm used to perform the hashing. If the hashing algorithm requires initialization or a key it should be set prior to calling this method</param>
        /// <returns>A base64 string of the hash value</returns>
        private string GenerateSignatureUsingHash(string signatureBase, HashAlgorithm hash)
            => ComputeHash(hash, signatureBase);

        /// <summary>
        /// Generates a signature using the HMAC-SHA1 algorithm  
        /// </summary>		
        /// <param name="url">The full url that needs to be signed including its non OAuth url parameters</param>
        /// <param name="consumerKey">The consumer key</param>
        /// <param name="consumerSecret">The consumer seceret</param>
        /// <param name="token">The token, if available. If not available pass null or an empty string</param>
        /// <param name="timeStamp"></param>
        /// <param name="nonce"></param>
        /// <param name="filterCreatedAtMin">The order's date filterCreatedAtMin, if available. For example - '2017-01-01'. If not available pass null or an empty string</param>
        /// <param name="status">The order's status, if available. If not available pass null or an empty string</param>
        /// <param name="tokenSecret">The token secret, if available. If not available pass null or an empty string</param>
        /// <param name="httpMethod">The http method used. Must be a valid HTTP method verb (POST,GET,PUT, etc)</param>
        /// <param name="normalizedUrl"></param>
        /// <param name="normalizedRequestParameters"></param>
        /// <returns>A base64 string of the hash value</returns>
        public string GenerateSignature(
            Uri url, string consumerKey, string consumerSecret, string token, string tokenSecret,
            string httpMethod, string timeStamp, string nonce, string filterCreatedAtMin, string status,
            out string normalizedUrl, out string normalizedRequestParameters)
        {
            return GenerateSignature(
                url, consumerKey, consumerSecret, token, tokenSecret,
                httpMethod, timeStamp, nonce, SignatureTypes.Hmacsha1, null, filterCreatedAtMin, status,
                out normalizedUrl, out normalizedRequestParameters);
        }



        public string GenerateSignature(
            Uri url, string consumerKey, string consumerSecret, string token, string tokenSecret,
            string httpMethod, string timeStamp, string nonce,
            string verifier, string filter, string status,
            out string normalizedUrl, out string normalizedRequestParameters)
        {
            return GenerateSignature(
                url, consumerKey, consumerSecret, token, tokenSecret,
                httpMethod, timeStamp, nonce, SignatureTypes.Hmacsha1, verifier, filter, status,
                out normalizedUrl, out normalizedRequestParameters);
        }


        /// <summary>
        /// Generates a signature using the specified signatureType 
        /// </summary>		
        /// <param name="url">The full url that needs to be signed including its non OAuth url parameters</param>
        /// <param name="consumerKey">The consumer key</param>
        /// <param name="consumerSecret">The consumer seceret</param>
        /// <param name="token">The token, if available. If not available pass null or an empty string</param>
        /// <param name="tokenSecret">The token secret, if available. If not available pass null or an empty string</param>
        /// <param name="httpMethod">The http method used. Must be a valid HTTP method verb (POST,GET,PUT, etc)</param>
        /// <param name="nonce"></param>
        /// <param name="signatureType">The type of signature to use</param>
        /// <param name="timeStamp"></param>
        /// <param name="verifier"></param>
        /// <param name="filter"></param>
        /// <param name="status"></param>
        /// <param name="normalizedUrl"></param>
        /// <param name="normalizedRequestParameters"></param>
        /// <returns>A base64 string of the hash value</returns>
        public string GenerateSignature(
            Uri url, string consumerKey, string consumerSecret, string token, string tokenSecret,
            string httpMethod, string timeStamp, string nonce, SignatureTypes signatureType,
            string verifier, string filter, string status,
            out string normalizedUrl, out string normalizedRequestParameters)
        {
            normalizedUrl = null;
            normalizedRequestParameters = null;

            switch (signatureType)
            {
                case SignatureTypes.Plaintext:
                    return HttpUtility.UrlEncode($"{consumerSecret}&{tokenSecret}");

                case SignatureTypes.Hmacsha1:
                    string signatureBase =
                        GenerateSignatureBase(
                            url, consumerKey, token,
                            httpMethod, timeStamp, nonce, Hmacsha1SignatureType, verifier, filter, status,
                            out normalizedUrl, out normalizedRequestParameters);

                    HMACSHA1 hmacsha1 = new HMACSHA1();
                    hmacsha1.Key = Encoding.ASCII.GetBytes(
                        $"{UrlEncode(consumerSecret)}&{(string.IsNullOrEmpty(tokenSecret) ? "" : UrlEncode(tokenSecret))}");

                    return GenerateSignatureUsingHash(signatureBase, hmacsha1);

                case SignatureTypes.Rsasha1:
                    throw new NotImplementedException();

                default:
                    throw new ArgumentException("Unknown signature type", nameof(signatureType));
            }
        }


        /// <summary>
        /// Generate the timestamp for the signature        
        /// </summary>
        /// <returns></returns>
        public static string GenerateTimeStamp()
        {
            // Default implementation of UNIX time of the current UTC time
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

        /// <summary>
        /// Generate a nonce
        /// </summary>
        /// <returns></returns>
        public string GenerateNonce()
        {
            // Just a simple implementation of a random number between 123400 and 9999999
            return _random.Next(123400, 9999999).ToString();
        }

    }

}
