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
            MessageGroupMembers = new HashSet<MessageGroupMember>();
            MessageGroupInvitations = new HashSet<MessageGroupInvitation>();
        }

        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 4, 
            ErrorMessage = "Group name must be between 4 and 50 characters.")]
        public string MessageGroupName { get; set; } = String.Empty;

        [Required]
        public bool IsDeleted { get; set; } = false;

        [Required]
        public DateTime CreatedDate { get; set; }

        public DateTime? DeletedDate { get; set; }

        public virtual ICollection<Post>? Posts { get; set; }

        public virtual ICollection<MessageGroupMember>? MessageGroupMembers { get; set; }

        public virtual ICollection<MessageGroupInvitation>? MessageGroupInvitations { get; set; }
    }
}
