using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.DTO
{
     public record CatResponse (
     int Id, 
     string CatId,
     int Width,
     int Height, 
     byte[] Image, 
     DateTime Created)
    {
        public CatResponse() : this(default, default, default, default, default, default) 
        { 
        }
    }
}
