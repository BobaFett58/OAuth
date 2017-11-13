using System.Net;

namespace OAuth.Library
{
    using RestSharp;
    using System;

    public enum MethodType
    {
        GET = 0,
        POST = 1,
    }

    /// <summary>
    /// Demonstrates how to do OAuth1 authentication via RestSharp with query parameters (for a Wordpress site using Woo Commerce).
    /// </summary>
    public class OAuth1
    {
        public bool AuthenticateWithOAuth(MethodType methodType, string baseUrl, string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret,
            string verifier, string title = "", string status = "", string filter = "")
        {
            var restClient = new RestClient(baseUrl);
            var oAuth = new OAuthBase();
            var nonce = oAuth.GenerateNonce();
            var timeStamp = OAuthBase.GenerateTimeStamp();

            string sig = oAuth.GenerateSignature(new Uri(baseUrl), consumerKey, consumerSecret, accessToken, accessTokenSecret, nameof(methodType),
                timeStamp, nonce, verifier, title, filter, status, normalizedUrl: out var _, normalizedRequestParameters: out var _);

            var request = new RestRequest((Method)((int)methodType));

            request.AddParameter("oauth_consumer_key", consumerKey);

            if (!string.IsNullOrEmpty(verifier))
                request.AddParameter("oauth_verifier", verifier);

            request.AddParameter("oauth_signature_method", "HMAC-SHA1");

            if (!string.IsNullOrEmpty(accessToken))
                request.AddParameter("oauth_token", accessToken);

            request.AddParameter("oauth_nonce", nonce);
            request.AddParameter("oauth_timestamp", timeStamp);
            request.AddParameter("oauth_version", "1.0");
            request.AddParameter("oauth_signature", sig);



            if (!string.IsNullOrEmpty(status))
                request.AddParameter("status", status);

            if (!string.IsNullOrEmpty(filter))
                request.AddParameter("filter[created_at_min]", filter);


            if (methodType == MethodType.POST)
            {
                request.RequestFormat = DataFormat.Json;

                if (!string.IsNullOrEmpty(title))
                    request.AddParameter(nameof(title), title);
            }


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
