using DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.DTO
{
     public record CatRequest(
     int Id, 
     string CatId,
     int Width,
     int Height, 
     string Image, 
     string ImageHash,
     DateTime Created,
     ICollection<TagRequest> tagRequests)
    {
        public CatRequest() : this(default, default, default, default, default, default, default, default) 
        { 
        }
    }
}
