using System.Collections.Generic;
using System.Net;

namespace OAuth.Library
{

    using Newtonsoft.Json;
    using RestSharp;
    using System;

    public class OAuth1
    {
        private RootObject RootObject { get; set; }
        private string Content { get; set; }
        public int GetOrderCount() => RootObject?.orders?.Count ?? 0;
        public List<Order> GetOrders() => RootObject?.orders;
        public Order GetOrder(int orderNo)
            => RootObject.orders.Count < orderNo ? null : RootObject?.orders[orderNo - 1];
        public string GetContent() => Content;

        public bool AuthenticateWithOAuth(string baseUrl, string consumerKey, string consumerSecret, string signatureMethod, string oauthVersion,
            string status, string filter)
        {

            const string accessToken = @"";
            const string accessTokenSecret = @"";

            RestClient restClient = new RestClient(baseUrl);
            OAuthBase oAuth = new OAuthBase();
            string nonce = oAuth.GenerateNonce();
            string timeStamp = oAuth.GenerateTimeStamp();
            string normalizedUrl;
            string normalizedRequestParameters;
            string sig = oAuth.GenerateSignature(new Uri(baseUrl), consumerKey, consumerSecret, accessToken, accessTokenSecret, "GET",
                timeStamp, nonce, filter, status, out normalizedUrl, out normalizedRequestParameters);

            var request = new RestRequest(Method.GET);

            request.AddParameter("oauth_consumer_key", consumerKey);
            request.AddParameter("oauth_signature_method", signatureMethod);
            request.AddParameter("oauth_timestamp", timeStamp);
            request.AddParameter("oauth_nonce", nonce);
            request.AddParameter("oauth_version", oauthVersion);
            request.AddParameter("oauth_signature", sig);

            if (!string.IsNullOrEmpty(status))
                request.AddParameter("status", status);

            if (!string.IsNullOrEmpty(filter))
                request.AddParameter("filter[created_at_min]", filter);

            var response = restClient.Execute(request);


            if (response.StatusCode != HttpStatusCode.OK) return false;

            Content = response.Content;
            RootObject = JsonConvert.DeserializeObject<RootObject>(response.Content);
            return true;
        }
    }
}
