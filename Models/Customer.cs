using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMS.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }
        public string CustomerNIC { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerAddress { get; set; }
        public string? CustomerContactNo { get; set; }
        public long? CreatedBy { get; set; }
        public long? UpdatedBy { get; set; }
        public long? DeletedBy { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
        public DateTime? DeletedAt { get; set; }

        // Store NIC photo file path
        public string? NICPhotoPath { get; set; }

        // Navigation Properties
        public virtual ICollection<Item> Items { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
