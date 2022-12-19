using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HearYe.Shared
{
    public class Post
    {
        public Post()
        {
            Acknowledgements = new HashSet<Acknowledgement>();
        }

        [Key]
        [Required]
        public int Id { get; set; }

        public int? UserId { get; set; }

        public int? MessageGroupId { get; set; }

        [Required]
        [StringLength(255)]
        public string Message { get; set; } = String.Empty;

        [Required]
        public bool IsDeleted { get; set; } = false;

        [Required]
        public DateTime CreatedDate { get; set; }

        public DateTime? StaleDate { get; set; }

        public DateTime? DeletedDate { get; set; }

        public virtual User? User { get; set; }

        public virtual MessageGroup? MessageGroup { get; set; }

        public virtual ICollection<Acknowledgement>? Acknowledgements { get; set; }
    }
}
