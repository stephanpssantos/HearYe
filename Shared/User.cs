using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HearYe.Shared
{
    public class User
    {
        public User()
        {
            Posts = new HashSet<Post>();
            Acknowledgements = new HashSet<Acknowledgement>();
        }

        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public Guid AadOid { get; set; }

        [Required]
        public string DisplayName { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedDate { get; set; }

        public DateTime? LastModifiedDate { get; set; }

        public virtual ICollection<Post>? Posts { get; set;}

        public virtual ICollection<Acknowledgement>? Acknowledgements { get; set; }
    }
}
