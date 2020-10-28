using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;

namespace main
{
    public static class Logs
    {
        public static string pathLog = "";
        public static string filePrefix = "";
        private static IPAddress m_IpAdr = null;

        public static IPAddress IpAdr
        {
            get
            {
                if (m_IpAdr == null)
                    m_IpAdr = Dns.GetHostAddresses(Dns.GetHostName())[0];
                return m_IpAdr;
            }
        }

        public static void LogAdd(string readTh)
        {
            try
            {
                //                lock (lockObject)
                //                {
                FileStream fs;
                readTh = "IP: " + IpAdr + " Время: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss:fff") + " " +
                         readTh + Environment.NewLine;
                string Patch = FileName();
                FileInfo fi = new FileInfo(Patch);

                if (fi.Exists)
                {
                    fs = new FileStream(Patch, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                }
                else
                {
                    fs = new FileStream(Patch, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite);
                }

                byte[] write = Encoding.GetEncoding(1251).GetBytes(readTh);
                fs.Write(write, 0, write.Length);
                fs.Close();
                //                }
            }
            catch (Exception ex)
            {
                FileStream fs;
                string Patch = "C:\\AisGorod\\Exeption.txt";
                FileInfo fi = new FileInfo(Patch);
                if (fi.Exists)
                {
                    fs = new FileStream(Patch, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                }
                else
                {
                    fs = new FileStream(Patch, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite);
                }

                byte[] write = Encoding.GetEncoding(1251).GetBytes(ex.Message);
                fs.Write(write, 0, write.Length);
                fs.Close();
            }
        }
 
        private static string FileName()
        {
            string timeStr = DateTime.Now.ToShortDateString();
            timeStr = timeStr.Replace('/', '_');
            timeStr = timeStr.Replace('.', '_');
            string patch = pathLog + filePrefix + "log_" + timeStr + ".txt";
            return patch;
        }

    }
}