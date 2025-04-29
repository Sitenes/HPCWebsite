using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{

    public class ServerRentalOrder
    {
        public int Id { get; set; }

        [Required]
        public int PaymentId { get; set; }
        public Payment Payment { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Required]
        public int ServerId { get; set; }

        [ForeignKey(nameof(ServerId))]
        public Server? Server { get; set; }

        [Required]
        [MaxLength(200)]
        public string ServerName { get; set; }

        public string ServerSpecs { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }

    public enum OrderStatus
    {
        Pending = 0,
        Active = 1,
        Completed = 2,
        Cancelled = 3
    }

   
    public class BillingInformation
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "نام الزامی است")]
        public string FirstName { get; set; } = "";

        [Required(ErrorMessage = "نام خانوادگی الزامی است")]
        public string LastName { get; set; } = "";

        [Required(ErrorMessage = "آدرس ایمیل الزامی است")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "شماره تلفن الزامی است")]
        [RegularExpression(@"^[0-9]{11}$", ErrorMessage = "شماره تلفن باید 11 رقمی باشد")]
        public string PhoneNumber { get; set; } = "";

        public string? CompanyName { get; set; } = "";

        [Url(ErrorMessage = "فرمت آدرس وبسایت نامعتبر است")]
        public string? Website { get; set; } = "";

        [Required(ErrorMessage = "آدرس کامل الزامی است")]
        public string FullAddress { get; set; } = "";

        public string? Country { get; set; } = "";
        public string? Province { get; set; } = "";

        [Required(ErrorMessage = "کد پستی الزامی است")]
        public string PostalCode { get; set; } = "";

        [Required(ErrorMessage = "مدت زمان اجاره الزامی است")]
        [Range(1, 365, ErrorMessage = "مدت زمان اجاره باید بین 1 تا 365 روز باشد")]
        public int RentalDays { get; set; }

        public int UserId { get; set; } // برای ارتباط با کاربر
        [ForeignKey(nameof(UserId))]
        public User? User { get; set; } // برای ارتباط با کاربر
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
    public class Payment
    {
        public int Id { get; set; }

        [Required]
        public int BillingInformationId { get; set; }
        public BillingInformation BillingInformation { get; set; }

        public ServerRentalOrder ServerRentalOrder { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public PaymentMethod Method { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        [MaxLength(100)]
        public string TransactionId { get; set; }

        public DateTime PaymentDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }

    public enum PaymentMethod
    {
        Zarinpal,
        BankPasargad
        //Paypal // در صورت نیاز می‌توانید فعال کنید
    }

    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed
    }


    public class ZarinpalPaymentData
    {
        public string Authority { get; set; }
        public int Status { get; set; }
        public string RefId { get; set; }
        public string CardPan { get; set; }
        public string CardHash { get; set; }
        public string FeeType { get; set; }
        public decimal Fee { get; set; }
    }

    public class PasargadPaymentData
    {
        public string ReferenceId { get; set; }
        public string TransactionDate { get; set; }
        public string MaskedCardNumber { get; set; }
        public string InvoiceNumber { get; set; }
        public int Status { get; set; }
    }
}
