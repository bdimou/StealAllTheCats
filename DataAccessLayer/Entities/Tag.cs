using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Entities
{ 
    /// Represents a tag entity that describes a cat's temperament or breed.  
    /// </summary>  
    public class Tag
    {
        /// <summary>  
        /// Gets or sets the unique identifier for the tag.  
        /// </summary>  
        public int Id { get; set; }

        /// <summary>  
        /// Gets or sets the name of the tag, describing the cat's temperament or breed.  
        /// </summary>  
        public string Name { get; set; }

        /// <summary>  
        /// Gets or sets the timestamp of when the tag was created.  
        /// </summary>  
        public DateTime Created { get; set; }

        // Navigation property for many-to-many relationship  
        public ICollection<CatTag> CatTags { get; set; }
    }
}
