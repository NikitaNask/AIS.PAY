using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;

namespace ConsoleApplication
{
    public class POSAtolOnline
    {
        private static string token = "";
        private static string timestamp = "";
        private static string callbackUrl;
        private static bool openChange;

        private static DateTime timeToken;
        private static string m_UUID;
        private string exeptionMessage;
        private string status;

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
                    LoginPass(m_Config.url, m_Config.login, m_Config.password, out exeptionMessage);
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
        }

        public string SendPaymentATOL(ServiceConfig m_Config, kkmServiceRequest m_request, CultureInfo FormatProvider, out string UUID, out string exeptionMessage, out string status)
        {

            UUID = "Не присвоен";
            exeptionMessage = "";
            status = "";

            Logs.LogAdd("SendPaymentATOL()Пытаюсь отправить чек с групповой: " + m_request.groupOperId);

            exeptionMessage = GetToken();
            if (!string.IsNullOrEmpty(exeptionMessage)) return exeptionMessage;

            try
            {
                string url = m_Config.url + "/possystem/v4/" + m_Config.codeGroup + "/sell";


                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);


                request.Method = "POST";
                request.ContentType = "application/json; charset=utf-8";
                request.Headers.Add("Token", token);

                string summ = m_request.summ;
                //m_request.groupOperId = new Random().Next(100000);
                //m_request.groupOperId = 319979251;
                m_request.email = "kkt@kkt.ru";


                string pData = @"{   ""external_id"":""" + m_request.groupOperId + @""", 
                                                    ""receipt"":
                                                            {""client"":{""email"":""" + m_request.email + @"""},
                                                    ""company"":
                                                            {""email"":""" + m_Config.emailCompany + @""",
                                                             ""inn"":""" + m_Config.inn + @""",
                                                             ""payment_address"":""" + m_Config.paymentAddr + @"""},
                                                    ""items"":
                                                             [{""name"":""" + m_Config.nameServices + @""",
                                                               ""price"":" + summ + @",   
                                                               ""quantity"":1,
                                                               ""sum"":" + summ + @",
                                                               ""vat"":{""type"":""vat20""}}],
                                                    ""payments"":
                                                             [{""type"":1,
                                                               ""sum"":" + summ + @"}],
                                                               ""total"":" + summ + @"},
                                                    ""service"":
                                                               {""callbackUrl"":""" + m_Config.callBackUrl + @"""}, 
                                                                ""timestamp"":""" + timestamp + @"""}";



                byte[] ByteArr = Encoding.UTF8.GetBytes(pData);
                request.ContentLength = ByteArr.Length;
                request.GetRequestStream().Write(ByteArr, 0, ByteArr.Length);

                DataJSONPayload dataJSONPayload;

                if (m_Config.debugLevel > 10)
                    Logs.LogAdd("Отправленный чек: " + pData);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader myStreamReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(DataJSONPayload));
                        dataJSONPayload = (DataJSONPayload)jsonFormatter.ReadObject(myStreamReader.BaseStream);
                        UUID = dataJSONPayload.uuid;
                        status = dataJSONPayload.status;
                    }
                }
                if (m_Config.debugLevel > 3) Logs.LogAdd("Отправленная групповая: " + m_request.groupOperId + Environment.NewLine +
                                                            "Полученный token: " + token + Environment.NewLine +
                                                            "Полученный UID: " + UUID);
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
                            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(DataJSONPayload));
                            var errorDataJSONPayload = (DataJSONPayload)jsonFormatter.ReadObject(myStreamReader.BaseStream);
                            if (errorDataJSONPayload.error != null) exeptionMessage = errorDataJSONPayload.error.text;
                        }
                    }
                }
                catch (Exception)
                {
                }

                switch (temp.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        Logs.LogAdd("ОШИБКА: " + content
                                      + Environment.NewLine + "Номер групповой операции: " + m_request.groupOperId + " UUID: " + m_UUID);
                        break;
                }
            }
            catch (Exception ex)
            {
                Logs.LogAdd("ОШИБКА: " + ex.Message + " ОШИБКА 2 : ");
            }
            return UUID;
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
    }

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

    //public class Payload
    //{
    //    public string ecr_registration_number { get; set; }
    //    public long fiscal_document_attribute { get; set; }
    //    public long fiscal_document_number { get; set; }
    //    public long fiscal_receipt_number { get; set; }
    //    public string fn_number { get; set; }
    //    public string fns_site { get; set; }
    //    public string receipt_datetime { get; set; }
    //    public long shift_number { get; set; }
    //    public double total { get; set; }
    //}

    //public class DataJSONPayloadGet
    //{
    //    public string callback_url { get; set; }
    //    public string daemon_code { get; set; }
    //    public string device_code { get; set; }
    //    public object warnings { get; set; }
    //    public object error { get; set; }
    //    public string external_id { get; set; }
    //    public string group_code { get; set; }
    //    public Payload payload { get; set; }
    //    public string status { get; set; }
    //    public string uuid { get; set; }
    //    public string timestamp { get; set; }
    //    public string ecr_registration_number { get; set; }
    //    public long fiscal_document_attribute { get; set; }
    //    public long fiscal_document_number { get; set; }
    //    public long fiscal_receipt_number { get; set; }
    //    public string fn_number { get; set; }
    //    public string fns_site { get; set; }
    //    public string receipt_datetime { get; set; }
    //    public long shift_number { get; set; }
    //    public double total { get; set; }
    //}

}

