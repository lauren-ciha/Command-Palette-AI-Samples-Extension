using Microsoft.CommandPalette.Extensions;

namespace Helpers
{
    public class NotificationRaisedEventArgs
    {
        public string NotificationMessage { get; set; }
        public MessageState State { get; set; }
        public NotificationRaisedEventArgs(string message, MessageState state)
        {
            NotificationMessage = message;
            State = state;
        }
    }
}