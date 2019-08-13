using System;
using System.Text;
using System.Threading;
using System.IO;
using System.Net;
using System.Globalization;
using System.Net.Sockets;
using System.Text.RegularExpressions;


namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpClient tcp = new TcpClient();
            tcp.Connect("192.168.55.81", 6000);
            NetworkStream nt = tcp.GetStream();
            string str = "<request><groupOperId>319979257</groupOperId><KKMTYPE>ATOL</KKMTYPE><summ>0.16</summ><textCheck><email>111@aisgorod.ru</email><![CDATA[]]></textCheck><operatorName>ЛК</operatorName></request>";
            byte[] bStr = Encoding.GetEncoding(1251).GetBytes(str);
            nt.Write(bStr, 0, str.Length);


        }
    }
}
