using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Domain/Orders/OrderStatus.cs
namespace Domain.Orders
{
    public enum OrderStatus
    {
        Created = 1,    // 已创建
        Paid = 2,       // 已支付
        Shipped = 3,    // 已发货
        Delivered = 4,  // 已送达
        Cancelled = 5   // 已取消
    }
}