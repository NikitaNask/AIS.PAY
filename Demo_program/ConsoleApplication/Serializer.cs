using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using Newtonsoft.Json;
using Formatting = System.Xml.Formatting;

namespace ConsoleApplication
{
    public static class Serializer
    {
        public static string ToXml<T>(T source, Encoding encoding)
        {
            return ToXml<T>(source, encoding, Formatting.None);
        }

        public static string ToXml<T>(T source, Encoding encoding, Formatting formatting)
        {
            using (MemoryStream tmpStream = new MemoryStream())
            {
                XmlWriterSettings xmlSetting = new XmlWriterSettings()
                {
                    NewLineOnAttributes = false,
                    OmitXmlDeclaration = true,
                    Indent = false,
                    NewLineHandling = NewLineHandling.None,
                    Encoding = encoding,
                    NamespaceHandling = NamespaceHandling.OmitDuplicates
                };

                using (XmlWriter xmlWriter = XmlWriter.Create(tmpStream, xmlSetting))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    XmlSerializerNamespaces serializerNamespaces = new XmlSerializerNamespaces();
                    serializerNamespaces.Add("", "");
                    serializer.Serialize(xmlWriter, source, serializerNamespaces);
                }

                tmpStream.Position = 0;

                using (StreamReader reader = new StreamReader(tmpStream, encoding))
                {
                    return reader.ReadToEnd();
                }
            }
        }

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
        public static string ToSoap<T>(T source, Encoding encoding)
        {
            using (MemoryStream tmpStream = new MemoryStream())
            {
                SoapFormatter xmlFormatter = new SoapFormatter();
                xmlFormatter.Serialize(tmpStream, source);
                tmpStream.Position = 0;
                using (StreamReader reader = new StreamReader(tmpStream, encoding))
                {
                    return reader.ReadToEnd();
                }
            }
        }
        public static T FromSoap<T>(string source, Encoding encoding)
        {
            using (MemoryStream memStream = new MemoryStream(encoding.GetBytes(source)))
            {
                memStream.Position = 0;
                SoapFormatter deserializer = new SoapFormatter();
                return (T)deserializer.Deserialize(memStream);
            }
        }
        public static string Escape(string xml)
        {
            return xml.Replace(">", "&gt;").Replace("<", "&lt;").Replace("\t", "").Replace("\n", "");
        }
        public static string Unescape(string xml)
        {
            return xml.Replace("&gt;", ">").Replace("&lt;", "<").Replace("\t", "").Replace("\n", "");
        }
        public static string ToCDATA(string source)
        {
            return "<![CDATA[" + source + "]]>";
        }
        public static string FromCDATA(string source)
        {
            return source.Replace("<![CDATA[", "").Replace("]]>", "");
        }
        public static string ToJson<T>(this T arg)
        {
            return JsonConvert.SerializeObject(arg, Newtonsoft.Json.Formatting.Indented);
        }
        public static string ToJson<T>(this T arg, Encoding encoding)
        {
            return JsonConvert.SerializeObject(arg, Newtonsoft.Json.Formatting.Indented);
        }
        public static T FromJson<T>(string sJson)
        {
            return JsonConvert.DeserializeObject<T>(sJson);
        }
    }
}

