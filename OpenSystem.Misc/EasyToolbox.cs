#if false
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Formatting = Newtonsoft.Json.Formatting;
namespace Global {
    public partial class EasyToolbox {
        static EasyToolbox()
        {
        }
        public static string FullName(dynamic? x) {
            if (x is null) return "null";
            var fullName = ((object)x).GetType().FullName!;
            if (fullName.StartsWith("<>f__AnonymousType")) return "AnonymousType";
            return fullName.Split('`')[0];
        }
        public static string ToJson(dynamic x, bool indent = false) {
            return JsonConvert.SerializeObject(x, indent ? Formatting.Indented : Formatting.None);
        }
        public static dynamic? FromJson(string json) {
            if (String.IsNullOrEmpty(json)) return null;
            return JsonConvert.DeserializeObject(json, new JsonSerializerSettings {
                DateParseHandling = DateParseHandling.None
            });
        }
        public static T? FromJson<T>(string json, T? fallback = default(T)) {
            //if (String.IsNullOrEmpty(json)) return default(T);
            if (String.IsNullOrEmpty(json)) return fallback;
            return JsonConvert.DeserializeObject<T>(json);
        }
        public static byte[] ToBson(dynamic x) {
            MemoryStream ms = new MemoryStream();
            using (BsonWriter writer = new BsonWriter(ms)) {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, x);
            }
            return ms.ToArray();
        }
        public static dynamic? FromBson(byte[] bson) {
            if (bson == null) return null;
            MemoryStream ms = new MemoryStream(bson);
            using (BsonReader reader = new BsonReader(ms)) {
                JsonSerializer serializer = new JsonSerializer();
                return serializer.Deserialize(reader);
            }
        }
        public static T? FromBson<T>(byte[] bson) {
            if (bson == null) return default(T);
            MemoryStream ms = new MemoryStream(bson);
            using (BsonReader reader = new BsonReader(ms)) {
                JsonSerializer serializer = new JsonSerializer();
                return serializer.Deserialize<T>(reader);
            }
        }
        public static dynamic? FromObject(dynamic? x) {
            if (x == null) return null;
            var o = (dynamic)JObject.FromObject(new { x = x },
                new JsonSerializer {
                    DateParseHandling = DateParseHandling.None
                });
            return o.x;
        }
        public static T? FromObject<T>(dynamic x) {
#if false
            string json = ToJson(x);
            return FromJson<T>(json);
#else
            dynamic? o = FromObject(x);
            if (o == null) return default(T);
            return (T)(o.ToObject<T>());
#endif
        }
        public static string? ToXml(dynamic x) {
            if (x == null) return null;
            if (FullName(x) == "System.Xml.Linq.XElement") {
                return ((XElement)x).ToString();
            }
            XDocument? doc;
            if (FullName(x) == "System.Xml.Linq.XDocument") {
                doc = (XDocument)x;
            }
            else {
                string json = ToJson(x);
                doc = JsonConvert.DeserializeXmlNode(json)?.ToXDocument();
                //return "<?>";
            }
            return doc == null ? "null" : doc.ToStringWithDeclaration();
        }
        public static XDocument? FromXml(string xml) {
            if (xml == null) return null;
            XDocument doc = XDocument.Parse(xml);
            return doc;
        }
        public static string ToString(dynamic x) {
            if ((x as string) != null) {
                var s = (string)x;
                return s;
            }
            if (FullName(x) == "Newtonsoft.Json.Linq.JValue") {
                var value = (Newtonsoft.Json.Linq.JValue)x;
                try {
                    x = (DateTime)value;
                }
                catch (Exception)
                {
                }
            }
            if (FullName(x) == "System.Xml.Linq.XDocument" || FullName(x) == "System.Xml.Linq.XElement") {
                string xml = ToXml(x);
                return xml;
            }
            else if (FullName(x) == "System.DateTime") {
                return x.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz");
            }
            else {
                try {
                    string json = ToJson(x, true);
                    return json;
                }
                catch (Exception) {
                    return x.ToString();
                }
            }
        }
        public static void Print(dynamic? x, string? title = null) {
            if (title != null) Console.Write(title + ": ");
            Console.WriteLine(EasyToolbox.ToString(x));
        }
        public static void Log(dynamic? x, string? title = null) {
            if (title != null) Console.Error.Write(title + ": ");
            Console.Error.WriteLine(EasyToolbox.ToString(x));
        }
        public static XDocument ParseXml(string xml) {
            XDocument doc = XDocument.Parse(xml);
            return doc;
        }
        public static string GetRidirectUrl(string url) {
            Task<string> task = GetRidirectUrlTask(url);
            task.Wait();
            return task.Result;
        }
        private static async Task<string> GetRidirectUrlTask(string url) {
            HttpClient client;
            HttpResponseMessage response;
            try {
                client = new HttpClient();
                response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception) {
                return url;
            }
            string result = response.RequestMessage!.RequestUri!.ToString();
            response.Dispose();
            return result;
        }
    }
}
#endif