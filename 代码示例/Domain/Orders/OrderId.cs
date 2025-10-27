using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Domain/Orders/OrderId.cs
// Domain/Orders/OrderId.cs
namespace Domain.Orders
{
    /// <summary>
    /// 订单ID值对象 - 强类型ID
    /// </summary>
    public record OrderId
    {
        public Guid Value { get; }

        // 私有构造函数
        private OrderId(Guid value)
        {
            Value = value;
        }

        // 工厂方法 - 创建新ID
        public static OrderId New()
        {
            return new OrderId(Guid.NewGuid());
        }

        // 工厂方法 - 从Guid创建
        public static OrderId From(Guid value)
        {
            if (value == Guid.Empty)
                throw new ArgumentException("Order ID不能为空Guid", nameof(value));

            return new OrderId(value);
        }

        // 工厂方法 - 从字符串创建
        public static OrderId From(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Order ID不能为空", nameof(value));

            if (!Guid.TryParse(value, out var guid))
                throw new ArgumentException("无效的Guid格式", nameof(value));

            return From(guid);
        }

        // 隐式转换到Guid
        public static implicit operator Guid(OrderId id) => id.Value;

        // 隐式转换从Guid
        public static implicit operator OrderId(Guid value) => From(value);

        // 隐式转换到字符串
        public static implicit operator string(OrderId id) => id.ToString();

        // 添加：隐式转换从字符串
        public static implicit operator OrderId(string value) => From(value);

        // 重写ToString
        public override string ToString() => Value.ToString();

        // 验证方法
        public bool IsEmpty() => Value == Guid.Empty;

        // 比较方法
        public bool Equals(OrderId? other)
        {
            if (other is null) return false;
            return Value.Equals(other.Value);
        }

        public override int GetHashCode() => Value.GetHashCode();

        //# 完整的使用场景：
        public void UseCase()
        {



            // 1. 创建OrderId
            OrderId orderId = OrderId.New();

            // 2. 隐式转换为Guid（调用 implicit operator Guid）
            Guid guidValue = orderId;                    // OrderId → Guid

            // 3. 隐式转换回OrderId（调用 implicit operator OrderId）
            OrderId orderId2 = guidValue;                // Guid → OrderId

            // 4. 从字符串隐式转换（调用 implicit operator OrderId(string)）
            OrderId orderId3 = "a1b2c3d4-1234-5678-9012-abcdef123456";  // string → OrderId

            // 5. 隐式转换为字符串（调用 implicit operator string）
            string stringValue = orderId;                // OrderId → string

            // 6. 在方法参数中自动转换
            void ProcessOrder(Guid orderGuid) { }

            ProcessOrder(orderId);  // 自动调用 implicit operator Guid
        }
    }

    /// <summary>
    /// 订单ID扩展方法
    /// </summary>
    public static class OrderIdExtensions
    {
        public static OrderId ToOrderId(this Guid guid) => OrderId.From(guid);
        public static OrderId ToOrderId(this string str) => OrderId.From(str);
    }
}







