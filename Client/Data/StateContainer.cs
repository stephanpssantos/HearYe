using HearYe.Shared;

namespace HearYe.Client.Data
{
    public class StateContainer
    {
        public string? UserDbId { get; set; }

        public IEnumerable<MessageGroup>? UserGroups { get; set; }
    }
}
