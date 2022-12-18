using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HearYe.Shared
{
    public class MessageGroupMember
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public int MessageGroupId { get; set; }

        [Required]
        public int MessageGroupRoleId { get; set; }

        [Required]
        public int UserId { get; set; }

        public virtual MessageGroup? MessageGroup { get; set; }

        public virtual MessageGroupRole? MessageGroupRole { get; set; }

        public virtual User? User { get; set; }
    }
}
