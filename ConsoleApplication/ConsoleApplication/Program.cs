using System;
using System.Text;
using System.Threading;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using System.Net.Sockets;
using System.Globalization;

namespace ConsoleApplication
{
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
    public class kkmServiceResponse
    {
        [XmlElement("code")]
        public int code { get; set; }
        [XmlElement("message")]
        public string errorMessage { get; set; }
        public long groupOperId { get; set; }
        public string checkNumber { get; set; }


        public string Kkmserialnumber { get; set; }
        public string Kkmeklznumber { get; set; }
        public string Kkmshiftsessionnumber { get; set; }
        public decimal Kkmglobalsummpaymentfrom { get; set; }
        public decimal ShiftSummIncome { get; set; } // сумма продаж за смену
        public DateTime LastKPKDateTime { get; set; } // Вернуть дату и время последней фискальной операции

        public decimal shiftSummPayment { get; set; }
        public decimal shiftSummPaymentRevert { get; set; }
        public int operationDocumentNumber { get; set; }
        public long shiftReceiptRevertNumber { get; set; }
        public long shiftReceiptNumber { get; set; }
        public string operationNumberFPD { get; set; }
        public string registrationNumberKKT { get; set; } // регистрационный номер ккт
        public string countDocumentNotSentOFD { get; set; } // количество документов не отправленных в ОФД

        //public bool ReadyToPrint { get; set; } // Готовность ККМ к печати чеков
        //public int StatusCheck { get; set; } // Статус чека

        public string requestDate { get; set; }
        // Только для Атола онлайн
        public string atoluuid { get; set; }

        //public kkmServiceResponse()
        //{
        //    LastKPKDateTime = Constants.SQLNullDate;
        //}
    }

    [XmlRoot("request")]
    public class kkmServiceRequest
    {
        public long groupOperId { get; set; }
        public string email { get; set; }
        public string summ { get; set; }
        public string textCheck { get; set; }
        public string operatorName { get; set; }
        public string uuid { get; set; }
    }
    public class ClientObject
    {

        public TcpClient client;
        ServiceConfig m_Config;
        public ClientObject(TcpClient tcpClient,ServiceConfig m_Config)
        {
            client = tcpClient;
            this.m_Config = m_Config;
        }
        private static string token = "";
        private static string timestamp = "";
        private static bool openChange;
        private string exeptionMessage;
        private static DateTime timeToken;

        public static Object SyncObj = new Object();

        private kkmServiceResponse SendInfoToKKM(kkmServiceRequest request, out string forceXml)
        {
            forceXml = null;
            string a_uuid;
            string a_exeptionMessage;
            string a_status;
            CultureInfo m_formatProvider = new CultureInfo("en-US", true);
            m_formatProvider.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
            m_formatProvider.NumberFormat.NumberDecimalSeparator = ".";


            kkmServiceResponse resp = new kkmServiceResponse();

                lock (POSAtolOnline.SyncObj)
                {
                    Logs.LogAdd("Данные для ККМ: m_Config.kkmTypeProtokol = " + m_Config.kkmTypeProtokol + " m_Config.kkmIPAddress = " + m_Config.kkmIPAddress + " m_Config.kkmPort = " +
                                    m_Config.kkmPort);

                    POSAtolOnline POSATOffline = new POSAtolOnline();
                    POSATOffline.m_Config = m_Config;


                    if (POSATOffline.SendPaymentATOL(m_Config, request, m_formatProvider, out a_uuid, out a_exeptionMessage, out a_status) != "")
                    {
                        Logs.LogAdd("Отправка на АТОЛОнлайн: ");
                        resp.Kkmserialnumber = "";
                        resp.Kkmeklznumber = "";
                        resp.Kkmshiftsessionnumber = "";
                        resp.Kkmglobalsummpaymentfrom = 0.0m;
                        resp.ShiftSummIncome = 0.0m;
                        resp.LastKPKDateTime = DateTime.Now;
                        resp.checkNumber = "";
                        resp.shiftSummPayment = 0.0m;
                        resp.shiftSummPaymentRevert = 0.0m;
                        resp.operationDocumentNumber = 0;
                        resp.operationNumberFPD = "";
                        resp.shiftReceiptNumber = 0;
                        resp.shiftReceiptRevertNumber = 0;
                        resp.countDocumentNotSentOFD = "";
                        resp.registrationNumberKKT = "";
                        resp.atoluuid = a_uuid;
                        if (m_Config.debugLevel > 1) Logs.LogAdd("Отправлена грууповая операция: " + request.groupOperId + " Возвращенный UUID: " + a_uuid);

                        resp.groupOperId = request.groupOperId;
                        resp.requestDate = DateTime.Now.ToString("yyyy-MM-ssTHH:mm:ss");
                        resp.code = 0;
                        resp.errorMessage = "";
                    }
                    else
                    {
                        Logs.LogAdd("Ошибка : ");
                    }


                    if (resp.code != 0) resp.errorMessage += " (code=" + resp.code + ")";

                    switch (resp.code)
                    {
                        case 0:
                            //всё хорошо
                            resp.code = 0;
                            break;
                        /*case 21:
                            //ошибка, повторите запрос на печать
                            resp.code = 1;
                            break;*/
                        default:
                            //фатальная ошибка
                            resp.code = 1;
                            break;
                    }
            }
            return resp;
        }


        public void Process()
        {
            NetworkStream stream = null;
            kkmServiceResponseAtol respAtol = new kkmServiceResponseAtol();
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
                string forceXml = "";
                kkmServiceRequest m_requests = Serializer.FromXml<kkmServiceRequest>(message);
                kkmServiceResponse m_response = SendInfoToKKM(m_requests, out forceXml);
                //GetPayment(, ref respAtol);

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
            ServiceConfig m_Config=new ServiceConfig();
            m_Config.debugLevel = 3;
            m_Config.url = "https://testonline.atol.ru";
            m_Config.callBackUrl = "http://testais.ru/?";
            m_Config.login = "v4-online-atol-ru";
            m_Config.password = "iGFFuihss";
            m_Config.codeGroup = "v4-online-atol-ru_4179";
            m_Config.inn = "5544332219";
            m_Config.emailCompany= "kkt@kkt.ru";
            m_Config.paymentAddr = "http://aisgorod.ru/";
            m_Config.nameServices = "Оплата услуг";

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
                    ClientObject clientObject = new ClientObject(client, m_Config);
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

