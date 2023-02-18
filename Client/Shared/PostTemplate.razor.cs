using Microsoft.AspNetCore.Components;
using HearYe.Shared;

namespace HearYe.Client.Shared
{
    public partial class PostTemplate
    {
        private bool acknowledgeDisabled = false;
        private bool isAcknowledged = false;
        private int acknowledgementCount = 0;
        private int postId = 0;

        [Parameter]
        public PostWithUserName? Post { get; set; }

        protected override void OnParametersSet()
        {
            if (Post is null || Post.Acknowledgements is null || Post.Id == postId)
            {
                return;
            }

            postId = Post.Id; // Prevents rerenders from resetting these values (unless it's a whole new post)
            acknowledgementCount = Post.Acknowledgements.Count;
            isAcknowledged = Post.Acknowledgements.Where(a => a.UserId.ToString() == State.UserDbId).FirstOrDefault()is not null;
        }

        private async void AcknowledgePost()
        {
            bool validDbId = int.TryParse(State.UserDbId, out int userDbId);
            if (Post is null || !validDbId)
            {
                return;
            }

            if (isAcknowledged)
            {
                DeletePostAcknowledgement(userDbId);
                return;
            }

            acknowledgeDisabled = true;
            Acknowledgement newAcknowledgement = new()
            {PostId = Post.Id, UserId = userDbId, CreatedDate = DateTimeOffset.Now, };
            HttpResponseMessage post = await Service.NewAcknowledgementAsync(newAcknowledgement);
            if (post.IsSuccessStatusCode)
            {
                isAcknowledged = true;
                acknowledgementCount++;
            }

            acknowledgeDisabled = false;
            StateHasChanged();
        }

        private async void DeletePostAcknowledgement(int userId)
        {
            if (!isAcknowledged)
            {
                return;
            }

            acknowledgeDisabled = true;
            HttpResponseMessage delete = await Service.DeleteAcknowledgementAsync(Post!.Id, userId);
            if (delete.IsSuccessStatusCode)
            {
                isAcknowledged = false;
                acknowledgementCount--;
            }

            acknowledgeDisabled = false;
            StateHasChanged();
        }

        private void OpenAcknowledgedList()
        {
            if (Post is null)
            {
                return;
            }

            State.AcknowledgedModalId = Post.Id;
            State.ModalIsOpen = true;
        }

        private string GetDisplayName()
        {
            if (Post is null)
            {
                return String.Empty;
            }

            if (Post.UserId is null || Post.DisplayName is null)
            {
                return "Unknown says:";
            }

            if (Post.UserId.ToString() == State.UserDbId)
            {
                return "You said:";
            }

            return Post.DisplayName + " says:";
        }

        // https://stackoverflow.com/questions/11/calculate-relative-time-in-c-sharp
        private static string GetPostTime(DateTimeOffset postTime)
        {
            const int SECOND = 1;
            const int MINUTE = 60 * SECOND;
            const int HOUR = 60 * MINUTE;
            const int DAY = 24 * HOUR;
            const int MONTH = 30 * DAY;
            var ts = new TimeSpan(DateTimeOffset.Now.Ticks - postTime.Ticks);
            double delta = Math.Abs(ts.TotalSeconds);
            if (delta < 1 * MINUTE)
                return "just now";
            if (delta < 2 * MINUTE)
                return "a minute ago";
            if (delta < 45 * MINUTE)
                return ts.Minutes + " minutes ago";
            if (delta < 90 * MINUTE)
                return "an hour ago";
            if (delta < 24 * HOUR)
                return ts.Hours + " hours ago";
            if (delta < 48 * HOUR)
                return "yesterday";
            if (delta < 30 * DAY)
                return ts.Days + " days ago";
            if (delta < 12 * MONTH)
            {
                int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? "one month ago" : months + " months ago";
            }
            else
            {
                int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
                return years <= 1 ? "one year ago" : years + " years ago";
            }
        }
    }
}