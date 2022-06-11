using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Haravan.ModelVc
{
    public class GHTK_Data_Product
    {
        public string name { get; set; }
        public decimal weight { get; set; }
        public int quantity { get; set; }
        public int price { get; set; }
        public string product_code { get; set; }
    }
    public class GHTK_Data_Order
    {
        public string id { get; set; }
        public string pick_name { get; set; }
        public string pick_address_id { get; set; }
        public string pick_address { get; set; }
        public string pick_province { get; set; }
        public string pick_district { get; set; }
        public string pick_ward { get; set; }
        public string pick_tel { get; set; }
        public string tel { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public string province { get; set; }
        public string district { get; set; }
        public string ward { get; set; }
        public string hamlet { get; set; }
        public string is_freeship { get; set; }
        public int pick_money { get; set; }
        public string note { get; set; }
        public int value { get; set; }
        public string transport { get; set; }
    }
    public class GHTK_Data{
        public List<GHTK_Data_Product> products { get; set; }
        public GHTK_Data_Order order { get; set; }
    }
}
