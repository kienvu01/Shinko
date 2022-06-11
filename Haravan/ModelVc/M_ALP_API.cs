using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Haravan.ModelVc
{
    public class M_ALP_API
    {
    }
    public class ALP_Data{
        public string ShopCode {get;set;}
        public int PaymentTypeId { get; set; }
        public int StructureId { get; set; }
        public int ServiceId { get; set; }
        public List<string> ServiceDVGTIds { get; set; }
        public int Weight { get; set; }

        public string PickingAddress { get; set; }
        public int FromProvinceId { get; set; }
        public int FromDistrictId { get; set; }
        public int FromWardId { get; set; }
        public string SenderName { get; set; }
        public string SenderPhone { get; set; }
        public string AddressNoteFrom { get; set; }

        public string ShippingAddress { get; set; }
        public int ProvinceId { get; set; }
        public int DistrictId { get; set; }
        public int WardId { get; set; }
        public int HubId { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverPhone { get; set; }
        public string AddressNoteTo { get; set; }
        public decimal COD { get; set; }
        public int Insured { get; set; }
        public string CusNote { get; set; }
        public string Content { get; set; }

    }
}
