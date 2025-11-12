using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Vehicle_Dealer_Management.DAL.Constants;

namespace Vehicle_Dealer_Management.DAL.Models
{
    public class SalesContract
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int QuoteId { get; set; }

        public int? OrderId { get; set; }

        [Required]
        public int DealerId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int CreatedBy { get; set; }

        [Required]
        [StringLength(40)]
        public string Status { get; set; } = SalesContractStatus.PendingCustomerSignature;

        [StringLength(2048)]
        public string? CustomerSignatureUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CustomerSignedAt { get; set; }

        public DateTime? DealerVerifiedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(QuoteId))]
        public virtual SalesDocument? Quote { get; set; }

        [ForeignKey(nameof(OrderId))]
        public virtual SalesDocument? Order { get; set; }

        [ForeignKey(nameof(DealerId))]
        public virtual Dealer? Dealer { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public virtual CustomerProfile? Customer { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public virtual User? CreatedByUser { get; set; }
    }
}


