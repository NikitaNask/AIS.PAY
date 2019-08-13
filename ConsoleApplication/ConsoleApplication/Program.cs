using System;
using System.Text;
using System.Threading;
using System.IO;
using System.Net;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using AIS.PAY.PaymentSendToKkm;
using System.Runtime.Serialization;
using AIS.PAY.Framework.Util;
using System.Web;
using System.Runtime.Serialization.Json;


namespace ConsoleApplication
{
    [DataContract]
    public class DataJSON
    {
        [DataMember]
        public string token { get; set; }
        [DataMember]
        public string error { get; set; }
        [DataMember]
        public string timestamp { get; set; }
        [DataMember]
        public string uuid { get; set; }
        [DataMember]
        public string status { get; set; }
    }
    [DataContract]
    public class DataJSONPayloadError
    {
        [DataMember]
        public string error_id { get; set; }
        [DataMember]
        public int code { get; set; }
        [DataMember]
        public string text { get; set; }
        [DataMember]
        public string type { get; set; }
    }
    [DataContract]
    public class DataJSONPayload
    {
        [DataMember]
        public string uuid { get; set; }
        [DataMember]
        public DataJSONPayloadError error { get; set; }
        [DataMember]
        public string status { get; set; }
        [DataMember]
        public string total { get; set; }
        [DataMember]
        public string fns_site { get; set; }
        [DataMember]
        public string fn_number { get; set; }
        [DataMember]
        public string shift_number { get; set; }
        [DataMember]
        public string receipt_datetime { get; set; }
        [DataMember]
        public string fiscal_receipt_number { get; set; }
        [DataMember]
        public string fiscal_document_number { get; set; }
        [DataMember]
        public string ecr_registration_number { get; set; }
        [DataMember]
        public string fiscal_document_attribute { get; set; }
        [DataMember]
        public string error_id { get; set; }
        [DataMember]
        public string code { get; set; }
        [DataMember]
        public string text { get; set; }
        [DataMember]
        public string type { get; set; }

    }
    public class request
    {
        public string groupOperId { get; set; }
        public string email { get; set; }
        public string summ { get; set; }
        public string textCheck { get; set; }
        public string operatorName { get; set; }
        public string uuid { get; set; }
    }
    public class kkmServiceResponseAtol
    {
        [XmlElement("code")]
        public int code { get; set; }
        [XmlElement("message")]
        public string errorMessage { get; set; }

        public string callback_url { get; set; }
        public string daemon_code { get; set; }
        public string device_code { get; set; }
        public object warnings { get; set; }
        public object error { get; set; }
        public string external_id { get; set; }
        public string group_code { get; set; }
        //public payload payload { get; set; }
        public string status { get; set; }
        public string uuid { get; set; }
        public string timestamp { get; set; }
        public string ecr_registration_number { get; set; }
        public long fiscal_document_attribute { get; set; }
        public long fiscal_document_number { get; set; }
        public long fiscal_receipt_number { get; set; }
        public string fn_number { get; set; }
        public string fns_site { get; set; }
        public string receipt_datetime { get; set; }
        public long shift_number { get; set; }
        public double total { get; set; }
    }
    public class ClientObject
    {

        public TcpClient client;
        public ClientObject(TcpClient tcpClient)
        {
            client = tcpClient;
        }
        private static string token = "";
        private static string timestamp = "";
        private static bool openChange;
        private string exeptionMessage;
        private static DateTime timeToken;
        //private static string m_UUID;
        //private string status;
        //private static string callbackUrl;

        public static Object SyncObj = new Object();
        public ServiceConfig m_Config { get; set; }

        private string GetToken()
        {
            try
            {
                if ((DateTime.UtcNow - timeToken).TotalHours >= 20)
                {
                    openChange = false;
                }
                if (!openChange)
                {
                    LoginPass("https://testonline.atol.ru", "v4-online-atol-ru", "iGFFuihss", out exeptionMessage);
                    if (exeptionMessage != "") return exeptionMessage;
                }
            }
            catch (WebException ex)
            {
                var temp = ex.Response as HttpWebResponse;

                string content;

                using (var r = new StreamReader(ex.Response.GetResponseStream()))
                    content = r.ReadToEnd();

                switch (temp.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        Logs.LogAdd("ОШИБКА: " + content);
                        break;
                }
            }
            return null;
        }
        private void LoginPass(string urlATOL, string login, string password, out string exeptionMessageLogin)
        {
            exeptionMessageLogin = "";
            try
            {
                string url = urlATOL + "/possystem/v4/getToken";


                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                request.Method = "POST";
                request.ContentType = "application/json; charset=utf-8";
                string pData = @"{""login"":""" + login + @""",""pass"":""" + password + @"""}";

                byte[] ByteArr = Encoding.UTF8.GetBytes(pData);
                request.ContentLength = ByteArr.Length;
                request.GetRequestStream().Write(ByteArr, 0, ByteArr.Length);

                DataJSON dataJSON;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader myStreamReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(DataJSON));
                        dataJSON = (DataJSON)jsonFormatter.ReadObject(myStreamReader.BaseStream);
                        Logs.LogAdd("Токен получен: " + dataJSON.token);
                    }
                }
                token = dataJSON.token;
                timestamp = dataJSON.timestamp;
                //timeToken = DateTime.ParseExact(dataJSON.timestamp, "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                timeToken = DateTime.UtcNow;
                openChange = true;
            }
            catch (WebException ex)
            {
                var temp = ex.Response as HttpWebResponse;

                string content;

                using (var r = new StreamReader(ex.Response.GetResponseStream()))
                    content = r.ReadToEnd();

                try
                {
                    using (var mm = new MemoryStream(Encoding.UTF8.GetBytes(content)))
                    {
                        using (StreamReader myStreamReader = new StreamReader(mm, Encoding.UTF8))
                        {
                            DataContractJsonSerializer jsonFormatter =
                                new DataContractJsonSerializer(typeof(DataJSONPayload));
                            var errorDataJSONPayload = (DataJSONPayload)jsonFormatter.ReadObject(myStreamReader.BaseStream);
                            if (errorDataJSONPayload.error != null) exeptionMessage = errorDataJSONPayload.error.text;
                        }
                    }
                }
                catch (Exception)
                { }

                switch (temp.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        Logs.LogAdd("ОШИБКА: " + content);
                        exeptionMessageLogin = content;
                        break;
                }
            }
            Console.ReadLine();
        }

        public string GetPayment(string uuid, ref kkmServiceResponseAtol kkmResponseAtol)
        {
            Logs.LogAdd("**************************** НАЧАЛО ОТВЕТА НА ЗАПРОС ФИСКАЛЬНЫХ ДАННЫХ ****************************");
            if (timeToken.Hour == 20)
            {
                openChange = false;
                GetToken();
            }

            exeptionMessage = GetToken();
            if (!string.IsNullOrEmpty(exeptionMessage)) return exeptionMessage;

            try
            {
                string url = m_Config.url + "/possystem/v4/" + m_Config.codeGroup + "/report/" + uuid + "?token=" + HttpUtility.UrlEncode(token);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);


                request.Method = "GET";
                request.ContentType = "application/json; charset=utf-8";
                request.AllowAutoRedirect = true;
                request.KeepAlive = true;
                request.Timeout = 30000;

                WebResponse resp = request.GetResponse();
                Stream dataStream = resp.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream, Encoding.UTF8);
                string outp = reader.ReadToEnd();

                using (var mm = new MemoryStream(Encoding.UTF8.GetBytes(outp)))
                {
                    DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(kkmServiceResponseAtol));
                    kkmResponseAtol = (kkmServiceResponseAtol)jsonFormatter.ReadObject(mm);
                    Logs.LogAdd("Ответ от АТОЛА фискальный документ: " + kkmResponseAtol.fiscal_document_number);
                }
            }

            catch (WebException ex)
            {
                var temp = ex.Response as HttpWebResponse;
                Logs.LogAdd("ОШИБКА: " + ex.Message);
                string content;

                using (var r = new StreamReader(ex.Response.GetResponseStream()))
                    content = r.ReadToEnd();

                try
                {
                    using (var mm = new MemoryStream(Encoding.UTF8.GetBytes(content)))
                    {
                        using (StreamReader myStreamReader = new StreamReader(mm, Encoding.UTF8))
                        {
                            DataContractJsonSerializer jsonFormatter =
                                new DataContractJsonSerializer(typeof(DataJSONPayload));
                            var errorDataJSONPayload = (DataJSONPayload)jsonFormatter.ReadObject(myStreamReader.BaseStream);
                            if (errorDataJSONPayload.error != null) exeptionMessage = errorDataJSONPayload.error.text;
                            Logs.LogAdd("Ошибка от АТОЛА : " + exeptionMessage);
                        }
                    }
                }
                catch (Exception)
                {
                }

                switch (temp.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        Logs.LogAdd("ОШИБКА: " + content);
                        break;
                }
            }
            Logs.LogAdd("**************************** КОНЕЦ ОТВЕТА НА ЗАПРОС ФИСКАЛЬНЫХ ДАННЫХ ****************************");
            return null;
        }

        //public string SendATOL(request m_request)
        //{



        //    string pData;
        //    RootObject rt = new RootObject();
        //    rt.external_id = m_request.groupOperId;
        //    rt.timestamp = "2019-01-01 00:00:000";
        //    rt.receipt = new Receipt();
        //    rt.receipt.client = new Client() { email = "ann1ann@mail.ru" };
        //    rt.receipt.company = new Company() { email = "ann1ann@mail.ru", inn = "731549876146", payment_address = "Димитровград, Ленина, 41В-28" };
        //    rt.receipt.items = new System.Collections.Generic.List<Item>();
        //    Item it = new Item()
        //    {
        //        name = "Коммунальные и иные услуги по ЕПД по лицевому счету № 89119160",
        //        price = 6931.56,
        //        quantity = 1,
        //        sum = 6931.56,
        //        payment_method = "full_payment",
        //        payment_object = "service"
        //    };
        //    it.vat = new Vat() { type = "vat20" };
        //    rt.receipt.items.Add(it);
        //    rt.receipt.payments = new System.Collections.Generic.List<Payment>();
        //    rt.receipt.payments.Add(new Payment() { sum = 10, type = 1 });
        //    pData = Serializer.ToJson<RootObject>(rt);
        //    return pData;
        //}

        public string SendPaymentATOL(ServiceConfig m_Config, request m_request, CultureInfo FormatProvider, out string UUID, out string exeptionMessage, out string status)
        {

            UUID = "Не присвоен";
            exeptionMessage = "";
            status = "";
            string pData;

            Logs.LogAdd("SendPaymentATOL()Пытаюсь отправить чек с групповой: " + m_request.groupOperId);

            exeptionMessage = GetToken();
            if (!string.IsNullOrEmpty(exeptionMessage)) return exeptionMessage;

            //try
            //{
                string url = m_Config.url + "/possystem/v4/" + m_Config.codeGroup + "/sell";


                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);


                request.Method = "POST";
                request.ContentType = "application/json; charset=utf-8";
                request.Headers.Add("Token", token);

                string summ = m_request.summ;
                //m_request.groupOperId = new Random().Next(100000);
                //m_request.groupOperId = 319979251;
                m_request.email = "kkt@kkt.ru";


                RootObject rt = new RootObject();
                rt.external_id = m_request.groupOperId;
                rt.timestamp = "2019-01-01 00:00:000";
                rt.receipt = new Receipt();
                rt.receipt.client = new Client() { email = "ann1ann@mail.ru" };
                rt.receipt.company = new Company() { email = "ann1ann@mail.ru", inn = "731549876146", payment_address = "Димитровград, Ленина, 41В-28" };
                rt.receipt.items = new System.Collections.Generic.List<Item>();
                Item it = new Item()
                {
                    name = "Коммунальные и иные услуги по ЕПД по лицевому счету № 89119160",
                    price = 6931.56,
                    quantity = 1,
                    sum = 6931.56,
                    payment_method = "full_payment",
                    payment_object = "service"
                };
                it.vat = new Vat() { type = "vat20" };
                rt.receipt.items.Add(it);
                rt.receipt.payments = new System.Collections.Generic.List<Payment>();
                rt.receipt.payments.Add(new Payment() { sum = 10, type = 1 });
                pData = Serializer.ToJson<RootObject>(rt);



                byte[] ByteArr = Encoding.UTF8.GetBytes(pData);
                request.ContentLength = ByteArr.Length;
                request.GetRequestStream().Write(ByteArr, 0, ByteArr.Length);

                //DataJSONPayload dataJSONPayload;

            //    if (m_Config.debugLevel > 10)
            //        Logs.LogAdd("Отправленный чек: " + pData);

            //    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            //    {
            //        using (StreamReader myStreamReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            //        {
            //            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(DataJSONPayload));
            //            dataJSONPayload = (DataJSONPayload)jsonFormatter.ReadObject(myStreamReader.BaseStream);
            //            UUID = dataJSONPayload.uuid;
            //            status = dataJSONPayload.status;
            //        }
            //    }
            //    if (m_Config.debugLevel > 3) Logs.LogAdd("Отправленная групповая: " + m_request.groupOperId + Environment.NewLine +
            //                                                "Полученный token: " + token + Environment.NewLine +
            //                                                "Полученный UID: " + UUID);
            //}
            //catch (WebException ex)
            //{
            //    var temp = ex.Response as HttpWebResponse;
            //    Logs.LogAdd("ОШИБКА: " + ex.Message);
            //    string content;

            //    using (var r = new StreamReader(ex.Response.GetResponseStream()))
            //        content = r.ReadToEnd();

            //    try
            //    {
            //        using (var mm = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            //        {
            //            using (StreamReader myStreamReader = new StreamReader(mm, Encoding.UTF8))
            //            {
            //                DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(DataJSONPayload));
            //                var errorDataJSONPayload = (DataJSONPayload)jsonFormatter.ReadObject(myStreamReader.BaseStream);
            //                if (errorDataJSONPayload.error != null) exeptionMessage = errorDataJSONPayload.error.text;
            //            }
            //        }
            //    }
            //    catch (Exception)
            //    {
            //    }

            //    switch (temp.StatusCode)
            //    {
            //        case HttpStatusCode.Unauthorized:
            //            Logs.LogAdd("ОШИБКА: " + content
            //                          + Environment.NewLine + "Номер групповой операции: " + m_request.groupOperId + " UUID: " + m_UUID);
            //            break;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Logs.LogAdd("ОШИБКА: " + ex.Message + " ОШИБКА 2 : ");
            //}
            return UUID;
        }

        public void Process()
        {
            NetworkStream stream = null;
            try
            {
                stream = client.GetStream();
                byte[] data = new byte[64]; // буфер для получаемых данных
                // получаем сообщение
                StringBuilder builder = new StringBuilder();
                int bytes = 0;
                do
                {
                    bytes = stream.Read(data, 0, data.Length);
                    builder.Append(Encoding.GetEncoding(1251).GetString(data, 0, bytes));
                }
                while (stream.DataAvailable);
                string message = builder.ToString();
                request m_req= Serializer.FromXml<request>(message);
                //string outAtol=SendATOL(m_req);
             

                Console.WriteLine(message);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (stream != null)
                    stream.Close();
                if (client != null)
                    client.Close();
            }
        }
    }
        class MyTcpListener
    {
        public static void Main()
        {
            TcpListener listener = null;
            try
            {
                Int32 port = 6000;
                IPAddress localAddr = IPAddress.Parse("192.168.55.81");
                listener = new TcpListener(localAddr, port);
                listener.Start();
                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                Console.Write("Waiting for a connection... ");

                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine("Connected!");
                    ClientObject clientObject = new ClientObject(client);
                    // создаем новый поток для обслуживания нового клиента
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                if (listener != null)
                    listener.Stop();
            }
            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }
    }
}

