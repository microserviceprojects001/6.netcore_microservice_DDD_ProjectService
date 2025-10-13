using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Domain/Exceptions/OrderNotFoundException.cs
namespace Domain.Exceptions
{
    public class OrderNotFoundException : Exception
    {
        public OrderId OrderId { get; }

        public OrderNotFoundException(OrderId orderId)
            : base($"订单不存在，ID: {orderId}")
        {
            OrderId = orderId;
        }

        public OrderNotFoundException(string orderId)
            : base($"订单不存在，ID: {orderId}")
        {
        }
    }

    public class InvalidOrderStateException : Exception
    {
        public InvalidOrderStateException(string message) : base(message)
        {
        }
    }

    public class SameAddressException : Exception
    {
        public SameAddressException(string message) : base(message)
        {
        }
    }

    public class EmptyOrderException : Exception
    {
        public EmptyOrderException(string message) : base(message)
        {
        }
    }

    public class MissingShippingAddressException : Exception
    {
        public MissingShippingAddressException(string message) : base(message)
        {
        }
    }
}