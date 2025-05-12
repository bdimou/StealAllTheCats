using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.DTO
{
    /// Represents a tag entity that describes a cat's temperament or breed.  
    /// </summary>  
    public record TagResponse(string Name)
    {
        public TagResponse() : this(string.Empty)
        {
        }
    }
}
