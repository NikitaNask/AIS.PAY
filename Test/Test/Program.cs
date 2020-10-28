using System;
using System.Text;
using System.Threading;
using System.IO;
using System.Net;
using System.Globalization;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Configuration;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            string ipAdress = ConfigurationManager.AppSettings["ipAdress"];
            string ipPort = ConfigurationManager.AppSettings["ipPort"];

            try
            {
                TcpClient tcp = new TcpClient();
                tcp.Connect(ipAdress, Convert.ToInt32(ipPort));
                NetworkStream nt = tcp.GetStream();
                string str = "<request><groupOperId>1122336</groupOperId><KKMTYPE>ATOL</KKMTYPE><summ>0.16</summ><textCheck><email>111@aisgorod.ru</email><![CDATA[]]></textCheck><operatorName>ЛК</operatorName><accountNumber>aqwe12</accountNumber></request>";
                byte[] bStr = Encoding.GetEncoding(1251).GetBytes(str);
                nt.Write(bStr, 0, str.Length);
                Console.ReadKey();
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
                Console.Read();
            }
        }
    }
}