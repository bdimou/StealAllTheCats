using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Entities
{
    public class CatTag
    {
        public int CatId { get; set; }
        public int TagId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(CatId))]
        public Cat Cat { get; set; }

        [ForeignKey(nameof(TagId))]
        public Tag Tag { get; set; }
    }
}
