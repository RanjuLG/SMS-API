using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMS.Models
{
    public class Karat
    {
        [Key]
        public int KaratId { get; set; }
        public int KaratValue { get; set; }
      //  public virtual ICollection<Pricing> Pricings { get; set; }

    }
}