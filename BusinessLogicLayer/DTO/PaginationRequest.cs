using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.DTO
{
    public class PaginationRequest
    {
        public string  PageIndex { get; set; }
        public string  PageSize { get; set; }
    }
}
