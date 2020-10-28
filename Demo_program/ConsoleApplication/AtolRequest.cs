using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication
{
    public class Client
    {
        public string email { get; set; }
    }

    public class Company
    {
        public string email { get; set; }
        public string inn { get; set; }
        public string payment_address { get; set; }
    }

    public class Vat
    {
        public string type { get; set; }
    }

    public class Item
    {
        public string name { get; set; }
        public decimal price { get; set; }
        public int quantity { get; set; }
        public decimal sum { get; set; }
        public Vat vat { get; set; }
    }

    public class Payment
    {
        public int type { get; set; }
        public decimal sum { get; set; }
    }

    public class Receipt
    {
        public Client client { get; set; }
        public Company company { get; set; }
        public IList<Item> items { get; set; }
        public IList<Payment> payments { get; set; }
        public decimal total { get; set; }
    }

    public class Service
    {
        public string callbackUrl { get; set; }
    }

    public class AtoRequest
    {
        public string external_id { get; set; }
        public Receipt receipt { get; set; }
        public Service service { get; set; }
        public string timestamp { get; set; }
    }

}
