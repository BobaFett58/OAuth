using OAuth.Library;

namespace OAuth
{
    class Program
    {
        static void Main(string[] args)
        {
            OAuth1 oauth = new OAuth1();

            var result = oauth.AuthenticateWithOAuth(
                baseUrl: "http://bearbarian.e-kei.pl/piotrsandbox/wc-api/v3/orders",
                //baseUrl: "http://bearbarian.e-kei.pl/piotrsandbox",
                consumerKey: "ck_1a1eeceba1b17fab472b3f10aa30453ddd815543",
                consumerSecret: "cs_4902de5e9dd164f3e9dc8ae47d50b765f7a0264d",
                signatureMethod: "HMAC-SHA1",
                oauthVersion: "1.0",
                status: "processing",
                filter: "2018-01-01"
                );

            if (result)
            {
                var content = oauth.GetContent(); //JSON FULL
                var orderCount = oauth.GetOrderCount(); // order count
                var orders = oauth.GetOrders(); //order list
                var order = oauth.GetOrder(1); // order numer 1  (zaczyna się liczba od 1)
            }
        }
    }
}
