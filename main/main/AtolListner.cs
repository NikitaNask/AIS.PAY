using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using System.Collections.Generic;
using System.Threading;
using System.Configuration;

namespace main
{
    public class kkmServiceResponseAtol
    {
        public long fiscal_document_number { get; set; }
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
        public string accountNumber { get; set; }

        public long groupOperId { get; set; }
        public string email { get; set; }
        public string summ { get; set; }

    }
    public class ClientObject
    {

        public TcpClient client;
        ServiceConfig m_Config;
        public ClientObject(TcpClient tcpClient, ServiceConfig m_Config)
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
            }
            catch (Exception ex)
            {
                Logs.LogAdd("Ошибка " + ex.Message);
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


    public partial class AtolListner : ServiceBase
    {
        protected bool isRun = false;


        public void Run()
        {
            TcpListener listener = null;
            ServiceConfig m_Config = new ServiceConfig();
            m_Config.debugLevel = 3;
            m_Config.url = "https://testonline.atol.ru";
            m_Config.callBackUrl = "http://testais.ru/?";
            m_Config.login = "v4-online-atol-ru";
            m_Config.password = "iGFFuihss";
            m_Config.codeGroup = "v4-online-atol-ru_4179";
            m_Config.inn = "5544332219";
            m_Config.emailCompany = "kkt@kkt.ru";
            m_Config.paymentAddr = "http://aisgorod.ru/";
            m_Config.nameServices = "Оплата услуг";
            Logs.pathLog = "c:\\aisgorod\\";

            try
            {
                m_Config.debugLevel= Convert.ToInt32(ConfigurationManager.AppSettings["debugLevel"]); 
                string ipAdress=ConfigurationManager.AppSettings["ipAdress"];
                string ipPort = ConfigurationManager.AppSettings["ipPort"];

                Int32 port = Convert.ToInt32(ipPort);
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");
                listener = new TcpListener(localAddr, port);
                listener.Start();
                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                //Console.Write("Waiting for a connection... ");
                Logs.LogAdd("Запуск листнера" + localAddr.Address.ToString()+ " порт:" + port.ToString());
                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    ClientObject clientObject = new ClientObject(client, m_Config);
                    // создаем новый поток для обслуживания нового клиента
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                    if (!isRun) break;
                }
            }
            catch (SocketException e)
            {
                Logs.LogAdd(e.Message);
            }
            finally
            {
                if (listener != null)
                    listener.Stop();
            }
        }

        protected override void OnStart(string[] args)
        {
            Logs.LogAdd("Вызов OnStart");
            new Thread(Run).Start();
            isRun = true;
        }

        protected override void OnStop()
        {
            isRun = false;
        }
    }
}
