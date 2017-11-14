
namespace OAuth.Library
{
    using RestSharp;
    using System;
    using System.Collections.Generic;

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
        public IRestResponse AuthenticateWithOAuth(MethodType method, OAuthSettings settings, List<KeyValuePair> requestParams)
        {
            var restClient = new RestClient(settings.BaseUrl);
            var oAuth = new OAuthBase();
            var nonce = oAuth.GenerateNonce();
            var timeStamp = OAuthBase.GenerateTimeStamp();

            string sig = oAuth.GenerateSignature(new Uri(settings.BaseUrl), settings.ConsumerKey, settings.ConsumerSecret, settings.AccessToken, settings.AccessTokenSecret, method.ToString(),
                timeStamp, nonce, settings.Verifier, requestParams, normalizedUrl: out var _, normalizedRequestParameters: out var _);

            var request = new RestRequest((Method)((int)method));

            request.AddParameter("oauth_consumer_key", settings.ConsumerKey);
            request.AddParameter("oauth_signature_method", "HMAC-SHA1");

            if (!string.IsNullOrEmpty(settings.Verifier))
                request.AddParameter("oauth_verifier", settings.Verifier);

            if (!string.IsNullOrEmpty(settings.AccessToken))
                request.AddParameter("oauth_token", settings.AccessToken);

            request.AddParameter("oauth_nonce", nonce);
            request.AddParameter("oauth_timestamp", timeStamp);
            request.AddParameter("oauth_version", "1.0");
            request.AddParameter("oauth_signature", sig);

            foreach (var param in requestParams)
            {
                request.AddParameter(param.Name, param.Value);
            }

            var response = restClient.Execute(request);

            return response;
        }
    }
}
