using BusinessLogicLayer.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.DTO
{
    
    public record CatTagRequest(
        CatRequest Cat,
        List<TagRequest> Tags
    )
    {
        public CatTagRequest() : this(default, default)
        {
        }
    }
}
