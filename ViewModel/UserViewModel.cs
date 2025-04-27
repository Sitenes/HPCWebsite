using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModel
{
    public class VerifyCodeRequest
    {
        public string Mobile { get; set; }
        public string Code { get; set; }
    }
    public class CodeViewModel
    {
        public long Mobile { get; set; }

        [Required(ErrorMessage = "کد الزامی است")]
        public string Code { get; set; }
    }
    public class SignUpViewModel
    {

        [Required(ErrorMessage = "نام الزامی است")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "نام خانوادگی الزامی است")]
        public string LastName { get; set; }
        public string Email { get; set; }
        [Required(ErrorMessage = "شماره تلفن الزامی است")]
        public int Id { get; set; }
    }

}
