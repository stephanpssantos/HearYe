using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HearYe.Shared
{
    public class MessageGroup
    {
        public MessageGroup() 
        {
            Posts = new HashSet<Post>();
            Acknowledgements = new HashSet<Acknowledgement>();
            MessageGroupMembers = new HashSet<MessageGroupMember>();
            MessageGroupInvitations = new HashSet<MessageGroupInvitation>();
        }

        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public string MessageGroupName { get; set; } = String.Empty;

        [Required]
        public bool IsDeleted { get; set; } = false;

        [Required]
        public DateTime CreatedDate { get; set; }

        public DateTime? DeletedDate { get; set; }

        public virtual ICollection<Post>? Posts { get; set; }

        public virtual ICollection<Acknowledgement>? Acknowledgements { get; set; }

        public virtual ICollection<MessageGroupMember>? MessageGroupMembers { get; set; }

        public virtual ICollection<MessageGroupInvitation>? MessageGroupInvitations { get; set; }
    }
}
