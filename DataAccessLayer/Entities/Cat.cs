using static System.Formats.Asn1.AsnWriter;
using System.ComponentModel;

namespace DataAccessLayer.Entities
{
    /// <summary>  
    /// Represents a Cat entity with properties for database storage and CaaS API integration.  
    /// </summary>  
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
        /// Contains the solution for storing the image.  
        /// </summary>  
        public byte[] Image { get; set; }

        /// <summary>  
        /// Timestamp of the creation of the database record.  
        /// </summary>  
        public DateTime Created { get; set; }

        // Navigation property for many-to-many relationship  
        public ICollection<CatTag> CatTags { get; set; }
    }
}
