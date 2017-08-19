//C# port of the https://github.com/kloon/WooCommerce-REST-API-Client-Library

//Including handling of woocommerce insisting on uppercase UrlEncoded entities

using RestSharp.Extensions.MonoHttp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace OAuth.Library
{
    public class WoocommerceApiClient

    {

        private static byte[] HashHMAC(byte[] key, byte[] message)

        {

            var hash = new HMACSHA256(key);

            return hash.ComputeHash(message);

        }



        private string Hash(string input)

        {

            using (SHA1Managed sha1 = new SHA1Managed())

            {

                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));

                var sb = new StringBuilder(hash.Length * 2);



                foreach (byte b in hash)

                {

                    // can be "x2" if you want lowercase

                    sb.Append(b.ToString("X2"));

                }



                return sb.ToString();

            }

        }



        public const string API_ENDPOINT = "wc-api/v3/";

        public string ApiUrl { get; set; }

        public string ConsumerSecret { get; set; }

        public string ConsumerKey { get; set; }

        public bool IsSsl { get; set; }



        public WoocommerceApiClient(string consumerKey, string consumerSecret, string storeUrl, bool isSsl = false)

        {

            if (string.IsNullOrEmpty(consumerKey) || string.IsNullOrEmpty(consumerSecret) ||

                string.IsNullOrEmpty(storeUrl))

            {

                throw new ArgumentException("ConsumerKey, consumerSecret and storeUrl are required");

            }

            this.ConsumerKey = consumerKey;

            this.ConsumerSecret = consumerSecret;

            this.ApiUrl = storeUrl.TrimEnd('/') + "/" + API_ENDPOINT;

            this.IsSsl = isSsl;

        }



        public string GetAllProducts()

        {

            return MakeApiCall("orders", new Dictionary<string, string>() { { "filter[created_at_min]", "2017-08-01" } });

        }

        public string GetProducts()

        {

            return MakeApiCall("orders");

        }



        private string MakeApiCall(string endpoint, Dictionary<string, string> parameters = null, string method = "GET")

        {

            if (parameters == null)

            {

                parameters = new Dictionary<string, string>();

            }

            parameters["oauth_consumer_key"] = this.ConsumerKey;

            parameters["oauth_timestamp"] =

                DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds.ToString();

            parameters["oauth_timestamp"] = parameters["oauth_timestamp"].Substring(0,

                parameters["oauth_timestamp"].IndexOf("."));

            parameters["oauth_nonce"] = Hash(parameters["oauth_timestamp"]);

            parameters["oauth_signature_method"] = "HMAC-SHA256";

            parameters["oauth_signature"] = GenerateSignature(parameters, method, endpoint);

            WebClient wc = new WebClient();

            StringBuilder sb = new StringBuilder();

            foreach (var pair in parameters)

            {

                sb.AppendFormat("&{0}={1}", HttpUtility.UrlEncode(pair.Key), HttpUtility.UrlEncode(pair.Value));

            }

            var url = this.ApiUrl + endpoint + "?" + sb.ToString().Substring(1).Replace("%5b", "%5B").Replace("%5d", "%5D");

            var result = wc.DownloadString(url);

            return result;

        }



        private string GenerateSignature(Dictionary<string, string> parameters, string method, string endpoint)

        {

            var baserequesturi = Regex.Replace(HttpUtility.UrlEncode(this.ApiUrl + endpoint), "(%[0-9a-f][0-9a-f])", c => c.Value.ToUpper());

            var normalized = NormalizeParameters(parameters);



            var signingstring = string.Format("{0}&{1}&{2}", method, baserequesturi,

                string.Join("%26", normalized.OrderBy(x => x.Key).ToList().ConvertAll(x => x.Key + "%3D" + x.Value)));

            var signature =

                Convert.ToBase64String(HashHMAC(Encoding.UTF8.GetBytes(this.ConsumerSecret),

                    Encoding.UTF8.GetBytes(signingstring)));

            Console.WriteLine(signature);

            return signature;

        }



        private Dictionary<string, string> NormalizeParameters(Dictionary<string, string> parameters)

        {

            var result = new Dictionary<string, string>();

            foreach (var pair in parameters)

            {

                var key = HttpUtility.UrlEncode(HttpUtility.UrlDecode(pair.Key));

                key = Regex.Replace(key, "(%[0-9a-f][0-9a-f])", c => c.Value.ToUpper()).Replace("%", "%25");

                var value = HttpUtility.UrlEncode(HttpUtility.UrlDecode(pair.Value));

                value = Regex.Replace(value, "(%[0-9a-f][0-9a-f])", c => c.Value.ToUpper()).Replace("%", "%25");

                result.Add(key, value);

            }

            return result;

        }

    }
}