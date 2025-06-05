using Entities.Models.MainEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    public class HpcUser
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        [MaxLength(11, ErrorMessage = "شماره موبایل معتبر نیست")]
        [RegularExpression(@"^09\d{9}$", ErrorMessage = "شماره موبایل معتبر نیست")]
        public long Mobile { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        //public string? VerificationCode { get; set; }
        //public DateTime? VerificationCodeExpiry { get; set; }
        public bool IsMobileVerified { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        #region Relations
        public int? WorkflowUserId { get; set; }
        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
        #endregion
    }
}
