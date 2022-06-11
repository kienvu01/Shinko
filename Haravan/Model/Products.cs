using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Haravan.Model
{
    public class HaravanItem
    {
        public string product_id { get; set; }
        public string variant_id { get; set; }
        public string ma_vt { get; set; }
    }
    public class Productss
    {
    }
    public class ProductItemFulliment
    {
        public string id { get; set; }
        public int quantity { get; set; }
    }
    public class LineOrderDetail
    {
        public string id { get; set; }
        public string product_id { get; set; }
        public string barcode { get; set; }
        public string variant_id { get; set; }
        public int quantity { get; set; }
    }
}
