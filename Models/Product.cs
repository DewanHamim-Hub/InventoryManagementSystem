using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IMS.Models
{
    public class Product
    {
        [Key]
        public int ProductID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Required]
        public int Quantity { get; set; }

        /// <summary>
        /// Threshold level for low-stock warning
        /// </summary>
        [Required]
        public int Threshold { get; set; }

        /// <summary>
        /// Computed property – NOT stored in database
        /// </summary>
        [NotMapped]
        public bool IsLowStock => Quantity <= Threshold;

        // Navigation Properties
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
        public ICollection<Restock> Restocks { get; set; } = new List<Restock>();
    }
}