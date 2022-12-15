using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HearYe.Shared
{
    public class User
    {
        [Key]
        [Required]
        public int UserId { get; set; }

        [Required]
        public Guid AadOid { get; set; }

        [Required]
        public string DisplayName { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedDate { get; set; }

        public DateTime? LastModifiedDate { get; set; }   
    }
}
