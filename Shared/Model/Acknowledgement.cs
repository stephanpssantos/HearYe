using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HearYe.Shared
{
    public class Acknowledgement
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public int PostId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTimeOffset CreatedDate { get; set; }

        public virtual User? User { get; set; }

        public virtual Post? Post { get; set; }
    }
}
