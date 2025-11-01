using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vehicle_Dealer_Management.DAL.Models
{
    public class Vehicle
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Make { get; set; } = string.Empty; // Hãng xe (Toyota, Honda, etc.)

        [Required]
        [StringLength(100)]
        public string Model { get; set; } = string.Empty; // Model xe

        [Required]
        public int Year { get; set; } // Năm sản xuất

        [StringLength(50)]
        public string? Color { get; set; } // Màu sắc

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } // Giá bán

        [StringLength(17)]
        public string? VinNumber { get; set; } // Số khung xe

        [StringLength(20)]
        public string? LicensePlate { get; set; } // Biển số xe

        [StringLength(50)]
        public string? EngineNumber { get; set; } // Số máy

        [StringLength(500)]
        public string? Description { get; set; } // Mô tả

        public bool IsAvailable { get; set; } = true; // Trạng thái có sẵn để bán

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        public virtual ICollection<Sale>? Sales { get; set; }
    }
}

