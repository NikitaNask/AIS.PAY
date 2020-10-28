using System;
using System.ComponentModel;

namespace main
{
    [Serializable]
	public class ServiceConfig //: SqlServerTcpServiceConfiguration
    {
        private int m_HoursStart;
        private int m_MinutesStart;
        private int m_debugLevel;
        private string m_kkmType;
        private string m_kkmTypeConnect;
        private string m_kkmIPAddress;
        private int m_kkmPort;
        private int m_kkmTimeout;
        private int m_kkmTypeProtokol;

        private string m_url;
        private string m_login;
        private string m_password;
        private string m_codeGroup;
        private string m_inn;
        private string m_paymentAddr;
        private string m_emailCompany;
        private string m_nameServices;
	    private string m_callBackUrl;

        [Category("ККМ")]
        [Description("IP Адрес ККМ")]
        public string kkmIPAddress
        {
            get { return m_kkmIPAddress; }
            set { m_kkmIPAddress = value; }
        }

        [Category("ККМ")]
        [Description("Порт ККМ")]
        public int kkmPort
        {
            get { return m_kkmPort; }
            set { m_kkmPort = value; }
        }

        [Category("ККМ")]
        [Description("Тип протокола ККМ")]
        public int kkmTypeProtokol
        {
            get { return m_kkmTypeProtokol; }
            set { m_kkmTypeProtokol = value; }
        }

        [Category("АТОЛ-Онлайн")]
        [Description("URL")]
        public string url
        {
            get { return m_url; }
            set { m_url = value; }
        }

        [Category("АТОЛ-Онлайн")]
        [Description("Login")]
        public string login
        {
            get { return m_login; }
            set { m_login = value; }
        }

        [Category("АТОЛ-Онлайн")]
        [Description("Password")]
        public string password
        {
            get { return m_password; }
            set { m_password = value; }
        }

        [Category("АТОЛ-Онлайн")]
        [Description("Код группы")]
        public string codeGroup
        {
            get { return m_codeGroup; }
            set { m_codeGroup = value; }
        }

        [Category("АТОЛ-Онлайн")]
        [Description("ИНН")]
        public string inn
        {
            get { return m_inn; }
            set { m_inn = value; }
        }

        [Category("АТОЛ-Онлайн")]
        [Description("Адрес расчетов")]
        public string paymentAddr
        {
            get { return m_paymentAddr; }
            set { m_paymentAddr = value; }
        }

        [Category("АТОЛ-Онлайн")]
        [Description("Почта компании")]
        public string emailCompany
        {
            get { return m_emailCompany; }
            set { m_emailCompany = value; }
        }

        [Category("АТОЛ-Онлайн")]
        [Description("Название услуги")]
        public string nameServices
        {
            get { return m_nameServices; }
            set { m_nameServices = value; }
        }
    
        [Category("АТОЛ-Онлайн")]
        [Description("Callback Url")]
        public string callBackUrl
        {
            get { return m_callBackUrl; }
            set { m_callBackUrl = value; }
        }

        [Category("АТОЛ-Онлайн")]
        [Description("Отладка")]
        public int debugLevel
        {
            get { return m_debugLevel; }
            set { m_debugLevel = value; }
        }
    }
}