using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Domain/Shared/Money.cs
namespace Domain.Shared
{
    public record Money
    {
        public decimal Amount { get; }
        public string Currency { get; } = "CNY";

        public static Money Zero => new Money(0m);

        public Money(decimal amount, string currency = "CNY")
        {
            if (amount < 0)
                throw new ArgumentException("金额不能为负数", nameof(amount));

            Amount = amount;
            Currency = currency ?? throw new ArgumentNullException(nameof(currency));
        }

        public static Money operator +(Money left, Money right)
        {
            if (left.Currency != right.Currency)
                throw new InvalidOperationException("货币类型不一致");

            return new Money(left.Amount + right.Amount, left.Currency);
        }

        public static Money operator *(Money money, int quantity)
        {
            return new Money(money.Amount * quantity, money.Currency);
        }

        public override string ToString() => $"{Amount:0.00} {Currency}";
    }
}