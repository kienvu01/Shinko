using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Haravan.Model
{
    public class Vochers
    {
    }
    public class API_Data_Voucher_ChechVoucher
    {
        public string fromDatdata { get; set; }
        public string toDate { get; set; }
        public List<string> so_ct { get; set; }
        //0:ok ,1:err
        public API_Data_Voucher_ChechVoucher() { }
    }
}
