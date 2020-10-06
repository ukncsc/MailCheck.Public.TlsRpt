using System;

namespace MailCheck.TlsRpt.Contracts.SharedDomain
{
    public abstract class Tag : IEquatable<Tag>
    {
        protected Tag(string type, string rawValue)
        {
            Type = type;
            RawValue = rawValue;
        }

        public string Type { get; }
        public string RawValue { get; }
        public string Explanation { get; set; }

        public override string ToString()
        {
            return $"{nameof(RawValue)}: {RawValue}{Environment.NewLine}{nameof(Explanation)}: {Explanation}";
        }

        public bool Equals(Tag other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(RawValue, other.RawValue) && string.Equals(Explanation, other.Explanation) && Type == other.Type;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Tag)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (RawValue != null ? RawValue.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Explanation != null ? Explanation.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Type != null ? Type.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(Tag left, Tag right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Tag left, Tag right)
        {
            return !Equals(left, right);
        }
    }
}