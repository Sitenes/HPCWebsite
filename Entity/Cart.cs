using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    public class HpcShoppingCart
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public HpcUser? User { get; set; }
        public List<HpcCartItem> Items { get; set; } = new List<HpcCartItem>();
        public List<HpcPayment> Payments { get; set; } = [];
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        [NotMapped]
        public decimal SubTotal => Items.Sum(i => i.TotalPrice);

        [NotMapped]
        public decimal DiscountAmount { get; set; }

        [NotMapped]
        public decimal Total => SubTotal - DiscountAmount;
        public int? WorkflowUserId { get; set; }
    }

    public class HpcCartItem
    {
        public int Id { get; set; }

        public int RentalDays { get; set; }
        public decimal DailyPrice { get; set; }
        public DateTime StartDate { get; set; }

        [NotMapped]
        public decimal TotalPrice => DailyPrice * RentalDays;

        public DateTime AddedAt { get; set; } = DateTime.Now;
        public int? WorkflowUserId { get; set; }

        public int ServerId { get; set; }
        [ForeignKey(nameof(ServerId))]
        public HpcServer? Server { get; set; }

        public int ShoppingCartId { get; set; }

        [ForeignKey(nameof(ShoppingCartId))]
        public HpcShoppingCart? ShoppingCart { get; set; }


    }
}
