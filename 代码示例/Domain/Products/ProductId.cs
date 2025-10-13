using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Domain/Products/ProductId.cs
namespace Domain.Products
{
    /// <summary>
    /// 产品ID值对象 - 强类型ID
    /// </summary>
    public record ProductId
    {
        public Guid Value { get; }

        // 私有构造函数
        private ProductId(Guid value)
        {
            Value = value;
        }

        // 工厂方法 - 创建新ID
        public static ProductId New()
        {
            return new ProductId(Guid.NewGuid());
        }

        // 工厂方法 - 从Guid创建
        public static ProductId From(Guid value)
        {
            if (value == Guid.Empty)
                throw new ArgumentException("Product ID不能为空Guid", nameof(value));

            return new ProductId(value);
        }

        // 工厂方法 - 从字符串创建
        public static ProductId From(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Product ID不能为空", nameof(value));

            if (!Guid.TryParse(value, out var guid))
                throw new ArgumentException("无效的Guid格式", nameof(value));

            return From(guid);
        }

        // 隐式转换到Guid
        public static implicit operator Guid(ProductId id) => id.Value;

        // 隐式转换从Guid
        public static implicit operator ProductId(Guid value) => From(value);

        // 隐式转换到字符串
        public static implicit operator string(ProductId id) => id.ToString();

        // 重写ToString
        public override string ToString() => Value.ToString();

        // 验证方法
        public bool IsEmpty() => Value == Guid.Empty;

        // 比较方法
        public bool Equals(ProductId? other)
        {
            if (other is null) return false;
            return Value.Equals(other.Value);
        }

        public override int GetHashCode() => Value.GetHashCode();
    }

    /// <summary>
    /// 产品ID扩展方法
    /// </summary>
    public static class ProductIdExtensions
    {
        public static ProductId ToProductId(this Guid guid) => ProductId.From(guid);
        public static ProductId ToProductId(this string str) => ProductId.From(str);
    }
}