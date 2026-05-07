using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IMS.Models
{
    public class Restock
    {
        [Key]
        public int RestockID { get; set; }

        [Required]
        public int QuantityAdded { get; set; }

        [Required]
        public DateTime RestockDate { get; set; }

        // Foreign Keys
        [ForeignKey("Product")]
        public int ProductID { get; set; }

        // Identity user (int key)
        public int UserID { get; set; }

        // Navigation Properties
        public Product? Product { get; set; }
        public ApplicationUser? User { get; set; }
    }
}