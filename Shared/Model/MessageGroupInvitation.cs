using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HearYe.Shared
{
    public class MessageGroupInvitation
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public int MessageGroupId { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Please enter a positive whole number.")]
        public int InvitedUserId { get; set; }

        [Required]
        public int InvitingUserId { get; set; }

        [Required]
        public bool InvitationActive { get; set; } = true;

        [Required]
        public bool InvitationAccepted { get; set; } = false;

        [Required]
        public DateTimeOffset CreatedDate { get; set; }

        public DateTimeOffset? ActionDate { get; set; }

        public virtual MessageGroup? MessageGroup { get; set; }

        public virtual User? InvitedUser { get; set; }

        public virtual User? InvitingUser { get; set; }
    }

    public class MessageGroupInvitationWithNames : MessageGroupInvitation
    {
        public string? MessageGroupName { get; set; }
        public string? InvitedUserName { get; set; }
        public string? InvitingUserName { get; set; }
    }
}
