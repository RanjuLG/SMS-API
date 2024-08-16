using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMS.Models
{
    public class Karat
    {
        public int KaratId { get; set; }
        public string KaratValue { get; set; }
        public virtual ICollection<Pricing> Pricings { get; set; }

    }
}