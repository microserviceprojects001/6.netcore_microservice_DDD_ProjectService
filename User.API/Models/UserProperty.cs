

namespace User.API.Models;

public class UserProperty
{
    /// <summary>
    /// 用户Id
    /// </summary>
    public int AppUserId { get; set; }

    public string Key { get; set; } = string.Empty;


    public string Value { get; set; } = string.Empty;


    public string Text { get; set; } = string.Empty;

    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        if (object.ReferenceEquals(this, obj))
        {
            return true;
        }

        UserProperty other = (UserProperty)obj;

        if (other.IsTransient() && this.IsTransient())
        {
            return false;
        }
        else
        {
            return other.Key == this.Key && other.Value == this.Value;
        }

    }

    public bool IsTransient()
    {
        return string.IsNullOrEmpty(this.Key) && string.IsNullOrEmpty(this.Value);
    }

    public static bool operator ==(UserProperty left, UserProperty right)
    {
        if (left is null && right is null)
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return left.Equals(right);
    }

    public static bool operator !=(UserProperty left, UserProperty right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        if (!IsTransient())
        {
            if (!_cachedHashCode.HasValue)
            {
                _cachedHashCode = (this.Key + this.Value).GetHashCode() ^ 31;
            }
            return _cachedHashCode.Value;
        }
        else
        {
            return base.GetHashCode();
        }

    }

    private int? _cachedHashCode;
}