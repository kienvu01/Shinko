using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Haravan.Model
{
    public class WP_Product
    {
    }
    public class WP_Product_att
    {
        public int id;
        public List<string> options;
        public bool variation;
        public bool visible;
        public WP_Product_att()
        {
            options = new List<string>();
            variation = true;
            visible = true;
        }
    }
    public class WP_Variant_att
    {
        public int id;
        public string option;
    }
    public class List_WP_ProductShinko
    {
        public List<WP_ProductShinko> create { get; set; }
    }

    public class Range_shinko_delete
    {
        public int start { get; set; }
        public int end { get; set; }
    }
    public class Shinko_list_att
    {
        public int id { get; set; }
    }
    public class List_WP_ProductAtrributeUpdate
    {
        public int id { get; set; }
        public List<Shinko_list_att> attributes { get; set; }
    }
    public class List_WP_ProductShinkoDelete
    {
        public List<int> delete { get; set; }
    }

    public class WP_CategoryShinko
    {
        public string name { get; set; }
    }

    public class WP_CategoryShinkoChild
    {
        public string name { get; set; }
        public int parent { get; set; }
    }

    public class WP_CateId
    {
        public int id { get; set; } 
    }
    public class WP_ProductShinko
    {

        public string name { get; set; }
        public string sku { get; set; }
        public bool manage_stock { get; set; }
        public string regular_price { get; set; }
        public string description { get; set; }
        public int stock_quantity { get; set; }
        public List<WP_CateId> categories { get;set ;}
        public List<atribute1> attributes { get; set; }
    }
    public class atribute1
    {
        public int id { get; set; }
        public List<string> options { get; set; }
        public string name { get; set; }
        public bool variation { get; set; }
        public bool visiable { get; set; }
        public int position { get; set; }   

    }

    public class WP_VariantShinko
        {
        
            public string description { get; set; }
            public string sku { get; set; }
            public bool manage_stock { get; set; }
            public string regular_price { get; set; }
            public int stock_quantity { get; set; }
            public List<WP_Variant_att> attributes { get; set; }
            public string type = "variable";
    }



        public class WP_ProductBodyToWP
        {
            public string name { get; set; }
            public string type { get; set; }
            public string status { get; set; }
            public string sku { get; set; }
            public decimal price { get; set; }
            public bool manage_stock { get; set; }
            public decimal stock_quantity { get; set; }
            public List<WP_Product_att> attributes { get; set; }
            public WP_ProductBodyToWP()
            {
                type = "variable";
                status = "publish";
                price = 0;
                manage_stock = true;
            }
        }
        public class WP_ListProductHasItemNotToWP
        {
            public string ma_vt { get; set; }
            public string wp_product { get; set; }
            public WP_ListProductHasItemNotToWP(string ma_vt, string wp_product)
            {
                this.ma_vt = ma_vt;
                this.wp_product = wp_product;
            }
        }
        public class WP_VariantBodyToWP
        {
            public string description { get; set; }
            public string status { get; set; }
            public string sku { get; set; }
            public decimal regular_price { get; set; }
            public bool manage_stock { get; set; }
            public decimal stock_quantity { get; set; }
            public List<WP_Variant_att> attributes { get; set; }
            public WP_VariantBodyToWP()
            {
                status = "publish";
                regular_price = 0;
                manage_stock = true;
            }
        }
        public class WP_productAttToWP
        {
            public int id { get; set; }
            public List<string> options { get; set; }
            public WP_productAttToWP()
            {
                options = new List<string>();
            }
        }
        public class Req_UpdateWP
        {
            public List<Req_updateWP_product> items { get; set; }
        }
        public class Req_updateWP_product
        {
            public string ma_vt { get; set; }
            public bool changeProduct { get; set; }
        }
    }
