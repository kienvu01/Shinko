using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Haravan.Model
{
    public class Product
    {
        public string barcode { get; set; }
        public int grams { get; set; }
        public double weight { get; set; }
        public double weight_unit { get; set; }

    }
}
