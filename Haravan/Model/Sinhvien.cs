using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Haravan.Model
{
    [Table(name: "dmsv")]
    public class Sinhvien
    {
        [Key]
        [Column(TypeName = "nchar(10)")]
        [Required]
        public string id { get; set; }

        [Column(TypeName = "nchar(100)")]
        public string name { get; set; }

        [Column(TypeName = "nchar(100)")]
        public string diachi { get; set; }
        public Sinhvien(string s1,string s2,string s3)
        {
            name = s1;
            id = s2;
            diachi = s3;
        }
        public Sinhvien() { }
    }
}
