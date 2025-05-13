using static System.Formats.Asn1.AsnWriter;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Entities
{
    /// <summary>  
    /// Represents a Cat entity with properties for database storage and CaaS API integration.  
    /// </summary>  
    [Index(nameof(ImageHash), IsUnique = true)]
    public class Cat
    {
        /// <summary>  
        /// An auto-incremental unique integer that identifies a cat within your database.  
        /// </summary>  
        public int Id { get; set; }

        /// <summary>  
        /// Represents the ID of the image returned from the CaaS API.  
        /// </summary>  
        public string CatId { get; set; }

        /// <summary>  
        /// Represents the width of the image returned from the CaaS API.  
        /// </summary>  
        public int Width { get; set; }

        /// <summary>  
        /// Represents the height of the image returned from the CaaS API.  
        /// </summary>  
        public int Height { get; set; }

        /// <summary>  
        /// Stores the image URL from the CaaS API.  
        /// </summary>  
        public string Image { get; set; }

        /// <summary>  
        /// Represents the hash of the image for quick comparison and storage efficiency.
        /// </summary>
        public string ImageHash { get; set; } 

        /// <summary>  
        /// Timestamp of the creation of the database record.  
        /// </summary>  
        public DateTime Created { get; set; }

        // Navigation property for many-to-many relationship  
        public ICollection<CatTag> CatTags { get; set; }
    }
}
