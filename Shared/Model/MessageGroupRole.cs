using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HearYe.Shared
{
    public class MessageGroupRole
    {
        public MessageGroupRole() 
        {
            MessageGroupMembers = new HashSet<MessageGroupMember>();
        }

        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public string RoleName { get; set; } = String.Empty;

        public virtual ICollection<MessageGroupMember>? MessageGroupMembers { get; set; }
    }
}
