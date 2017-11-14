
namespace OAuth.Library
{
    using RestSharp.Extensions.MonoHttp;

    public class KeyValuePair
    {
        public string Name { get; set; }
        public string Value { get; set; }

        private KeyValuePair() { }

        /// <summary>
        /// If Name has some characters like '[' or ']' it will replaced to percent-encoding. More read here -> https://en.wikipedia.org/wiki/Percent-encoding
        /// If it doesn't work with this method, try to replace small characters to large ones. (for ex.: " Replace("%5b", "%5B").Replace("%5d", "%5D") ").
        /// </summary>
        public static KeyValuePair Create(string name, string value)
        {
            return new KeyValuePair
            {
                Name = HttpUtility.UrlEncode(name),
                Value = value.Replace(" ", "%20") // space doesn't want to work normally
            };
        }
    }
}
