using System;

namespace MailCheck.TlsRpt.Contracts.SharedDomain
{
    public class Message : IEquatable<Message>
    {
        public Message(Guid id, string name, string source, MessageType messageType, string text, string markDown, MessageDisplay messageDisplay = MessageDisplay.Standard)
        {
            Id = id;
            Name = name;
            Source = source;
            MessageType = messageType;
            Text = text;
            MarkDown = markDown;
            MessageDisplay = messageDisplay;
        }

        public Guid Id { get; }
        public string Name { get; }
        public string Source { get; }

        public MessageType MessageType { get; }

        public string Text { get; }
        public string MarkDown { get; }
        public MessageDisplay MessageDisplay { get; }

        public bool Equals(Message other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Source, other.Source) &&
                   MessageType == other.MessageType &&
                   Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Message)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Source != null ? Source.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)MessageType;
                hashCode = (hashCode * 397) ^ (Text != null ? Text.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(Message left, Message right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Message left, Message right)
        {
            return !Equals(left, right);
        }
    }
}