using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameWork
{
    public class ResultViewModel
    {
        public dynamic? data { get; set; } = null;
        public string? message { get; set; }
        public bool status { get; set; }
        public int statusCode { get; set; }
        public int totalCount { get; set; }
        public int listSize { get; set; }
        public int listNumber { get; set; }


    }

}
