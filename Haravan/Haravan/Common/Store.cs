using Haravan.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Haravan.Haravan.Common
{

    public class Res_UpdateItem
    {
        public string ma_kho { get; set; }
        public List<string> line_items { get; set; }
    }
    public class LocationHaravan
    {
        public string id { get; set; }
        public string name { get; set; }
    }
    public class ListDL
    {
        public string ma_kho { get; set; }
        public List<Store_UpdateLineItem> lst_variant { get; set; }
    }

    public class ResUpdateDL
    {
        public string ma_kho { get; set; }
    }

    public class Req_UpdateInventoryItem
    {
        public List<string> lst_ma_vt { get; set; }
    }
    public class Store
    {
    }
}
