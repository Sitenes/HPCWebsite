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

}
