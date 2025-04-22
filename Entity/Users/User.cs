using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.Users
{
    public class User
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        [MaxLength(11)]
        [RegularExpression(@"^09\d{9}$", ErrorMessage = "شماره موبایل معتبر نیست")]
        public long Mobile { get; set; }
        [MaxLength(100)]
        public string? Email { get; set; }

        public string? VerificationCode { get; set; }
        public DateTime? VerificationCodeExpiry { get; set; }
        public bool IsMobileVerified { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
