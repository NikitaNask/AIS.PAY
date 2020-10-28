using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using Newtonsoft.Json;
using Formatting = System.Xml.Formatting;

namespace main
{
    public static class Serializer
    {

        public static T FromXml<T>(string source)
        {
            using (StringReader stringReader = new StringReader(source))
            {
                using (XmlTextReader xmlTextReader = new XmlTextReader(stringReader))
                {
                    XmlSerializer xmlFormatter = new XmlSerializer(typeof(T));
                    return (T)xmlFormatter.Deserialize(xmlTextReader);
                }
            }
        }
        public static string ToJson<T>(this T arg)
        {
            return JsonConvert.SerializeObject(arg, Newtonsoft.Json.Formatting.Indented);
        }
    }
}