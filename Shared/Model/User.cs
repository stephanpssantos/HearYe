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
            MessageGroups = new HashSet<MessageGroupMember>();
            MessageGroupInvitations = new HashSet<MessageGroupInvitation>();
            MessageGroupInvitationsSent = new HashSet<MessageGroupInvitation>();
        }

        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public Guid AadOid { get; set; }

        [Required]
        public string DisplayName { get; set; } = String.Empty;

        [Required]
        public bool AcceptGroupInvitations { get; set; } = true;

        public int? DefaultGroupId { get; set; }

        [Required]
        public bool IsDeleted { get; set; } = false;

        [Required]
        public DateTimeOffset CreatedDate { get; set; }

        public DateTimeOffset? LastModifiedDate { get; set; }

        public DateTimeOffset? DeletedDate { get; set; }

        public virtual MessageGroup? DefaultGroup { get; set; }

        public virtual ICollection<Post>? Posts { get; set;}

        public virtual ICollection<Acknowledgement>? Acknowledgements { get; set; }

        public virtual ICollection<MessageGroupMember>? MessageGroups { get; set; }

        public virtual ICollection<MessageGroupInvitation>? MessageGroupInvitations { get; set; }

        public virtual ICollection<MessageGroupInvitation>? MessageGroupInvitationsSent { get; set; }
    }

    public class UserPublicInfo
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = String.Empty;
        public bool AcceptGroupInvitations { get; set; } = true;
    }
}
