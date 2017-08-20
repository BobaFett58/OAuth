using System.Net;

namespace OAuth.Library
{
    using RestSharp;
    using System;


    /// <summary>
    /// Demonstrates how to do OAuth1 authentication via RestSharp with query parameters (for a Wordpress site using Woo Commerce).
    /// </summary>
    public class OAuth1
    {
        public bool AuthenticateWithOAuth(string baseUrl, string consumerKey, string consumerSecret,
            string status, string filter)
        {

            const string accessToken = @"";
            const string accessTokenSecret = @"";

            var restClient = new RestClient(baseUrl);
            var oAuth = new OAuthBase();
            var nonce = oAuth.GenerateNonce();
            var timeStamp = OAuthBase.GenerateTimeStamp();

            string sig = oAuth.GenerateSignature(new Uri(baseUrl), consumerKey, consumerSecret, accessToken, accessTokenSecret, "GET",
                timeStamp, nonce, filter, status, normalizedUrl: out var _, normalizedRequestParameters: out var _);

            var request = new RestRequest(Method.GET);

            request.AddParameter("oauth_consumer_key", consumerKey);
            request.AddParameter("oauth_signature_method", "HMAC-SHA1");
            request.AddParameter("oauth_nonce", nonce);
            request.AddParameter("oauth_version", "1.0");
            request.AddParameter("oauth_signature", sig);

            if (!string.IsNullOrEmpty(status))
                request.AddParameter("status", status);

            if (!string.IsNullOrEmpty(filter))
                request.AddParameter("filter[created_at_min]", filter);

            var response = restClient.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = response.Content;
                return true;
            }

            return false;
        }
    }
}
