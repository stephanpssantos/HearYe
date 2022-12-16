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
        public DateTime CreatedDate { get; set; }

        //[ForeignKey("UserId")]
        //[InverseProperty("Acknowledgements")]
        public virtual User? User { get; set; }

        //[ForeignKey("PostId")]
        //[InverseProperty("Acknowledgements")]
        public virtual Post? Post { get; set; }
    }
}
