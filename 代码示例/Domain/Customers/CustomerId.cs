using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Domain/Customers/CustomerId.cs
namespace Domain.Customers
{
    /// <summary>
    /// 客户ID值对象 - 强类型ID
    /// </summary>
    public record CustomerId
    {
        public Guid Value { get; }

        // 私有构造函数
        private CustomerId(Guid value)
        {
            Value = value;
        }

        // 工厂方法 - 创建新ID
        public static CustomerId New()
        {
            return new CustomerId(Guid.NewGuid());
        }

        // 工厂方法 - 从Guid创建
        public static CustomerId From(Guid value)
        {
            if (value == Guid.Empty)
                throw new ArgumentException("Customer ID不能为空Guid", nameof(value));

            return new CustomerId(value);
        }

        // 工厂方法 - 从字符串创建
        public static CustomerId From(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Customer ID不能为空", nameof(value));

            if (!Guid.TryParse(value, out var guid))
                throw new ArgumentException("无效的Guid格式", nameof(value));

            return From(guid);
        }

        // 隐式转换到Guid
        public static implicit operator Guid(CustomerId id) => id.Value;

        // 隐式转换从Guid
        public static implicit operator CustomerId(Guid value) => From(value);

        // 隐式转换到字符串
        public static implicit operator string(CustomerId id) => id.ToString();

        // 重写ToString
        public override string ToString() => Value.ToString();

        // 重写GetHashCode (record类型自动实现)
        // 重写Equals (record类型自动实现)

        // 验证方法
        public bool IsEmpty() => Value == Guid.Empty;

        // 比较方法
        public bool Equals(CustomerId? other)
        {
            if (other is null) return false;
            return Value.Equals(other.Value);
        }

        public override int GetHashCode() => Value.GetHashCode();
    }

    /// <summary>
    /// 客户ID扩展方法
    /// </summary>
    public static class CustomerIdExtensions
    {
        public static CustomerId ToCustomerId(this Guid guid) => CustomerId.From(guid);
        public static CustomerId ToCustomerId(this string str) => CustomerId.From(str);
    }
}