using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Haravan.Haravan.Common
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
    public class Res_LineCreatNewItem
    {
        public string ma_vt { get; set; }
        public string ten_vt { get; set; }
        public decimal gram { get; set; }
        public decimal price { get; set; }
        public string option1 { get; set; }
        public string option2 { get; set; }
        public string url { get; set; }
        public string loai_vt { get; set; }

    }
    public class Res_CreatNewItem
    {
        public string ten_sp { get; set; }
        public string content { get; set; }
        public string tag { get; set; }
        public string loai_vt { get; set; }
        public bool published { get; set; }
        public List<Res_LineCreatNewItem> line_item { get; set; }
        public Res_CreatNewItem()
        {
            line_item = new List<Res_LineCreatNewItem>();
        }
    }
    public class Img_CreatNewItem
    {
        public string src { get; set; }
    }
    public class CreatNewItem_Varian
    {
        public string barcode { get; set; }
        public decimal grams { get; set; }
        public string inventory_management { get; set; }
        public decimal price { get; set; }
        public bool requires_shipping { get; set; }
        public string sku { get; set; }
        public string title { get; set; }
        public string option1 { get; set; }
        public string option2 { get; set; }
        public string option3 { get; set; }
    }
    public class Req_UpdateHaravan
    {
        public List<Req_updateHaravan_product> items { get; set; }
    }
    public class Req_updateHaravan_product
    {
        public string ma_vt { get; set; }
        public bool changeProduct { get; set; }
    }
}
