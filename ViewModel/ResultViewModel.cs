using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels
{
    public class ResultViewModel<T>
    {
        public ResultViewModel(T? Data = default, int totalCount = 0, int listSize = 0, int listNumber = 0)
        {
            var statusCode = Status;
            Message = "";
            this.Data = Data;
            TotalCount = totalCount;
            ListSize = listSize;
            ListNumber = listNumber;
        }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public bool Status => StatusCode.ToString().StartsWith("2") || StatusCode.ToString().StartsWith("3");
        public int StatusCode { get; set; } = 200;
        public int TotalCount { get; set; }
        public int ListSize { get; set; }
        public int ListNumber { get; set; }

    }
}
