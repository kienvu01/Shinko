using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Haravan.Haravan.Common
{
    public class LineItem
    {
        public string variant_id { get; set; }
        public string product_id { get; set; }
        public double price { get; set; }
        public double quantity { get; set; }
        public double price_original { get; set; }
        public LineItem(string _variant_id, string _product_id, double _price, double _quantity, double _price_original)
        {
            variant_id = _variant_id;
            product_id = _product_id;
            price = _price;
            quantity = _quantity;
            price_original = _price_original;
        }

    }
    public class ProductToHaravanAll
    {
        public string ma_vt { get; set; }
        public string har_product { get; set; }
        public decimal so_luong { get; set; }
    }
    public class ProductVariantToHaravanAll
    {
        public string ma_vt { get; set; }
        public string har_product { get; set; }
        public decimal so_luong { get; set; }
    }
}
