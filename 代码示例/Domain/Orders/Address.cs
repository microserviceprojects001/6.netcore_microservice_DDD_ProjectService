using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Domain/Orders/Address.cs
namespace Domain.Orders
{
    public class Address : ValueObject
    {
        public string Province { get; }
        public string City { get; }
        public string District { get; }
        public string Street { get; }
        public string ZipCode { get; }

        // 私有构造函数
        private Address() { } // EF Core需要

        // 公有构造函数
        public Address(string province, string city, string district, string street, string zipCode)
        {
            if (string.IsNullOrWhiteSpace(province))
                throw new ArgumentException("省份不能为空", nameof(province));
            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException("城市不能为空", nameof(city));
            if (string.IsNullOrWhiteSpace(street))
                throw new ArgumentException("街道不能为空", nameof(street));

            Province = province.Trim();
            City = city.Trim();
            District = district?.Trim();
            Street = street.Trim();
            ZipCode = zipCode?.Trim();

            Validate();
        }

        // 验证逻辑
        private void Validate()
        {
            if (Province.Length > 50)
                throw new ArgumentException("省份名称过长");
            if (City.Length > 50)
                throw new ArgumentException("城市名称过长");
            // 更多验证规则...
        }

        // 业务方法：创建新地址（整体替换）
        public Address WithProvince(string newProvince)
        {
            return new Address(newProvince, City, District, Street, ZipCode);
        }

        public Address WithCity(string newCity)
        {
            return new Address(Province, newCity, District, Street, ZipCode);
        }

        public Address WithDistrict(string newDistrict)
        {
            return new Address(Province, City, newDistrict, Street, ZipCode);
        }

        public Address WithStreet(string newStreet)
        {
            return new Address(Province, City, District, newStreet, ZipCode);
        }

        public Address WithZipCode(string newZipCode)
        {
            return new Address(Province, City, District, Street, newZipCode);
        }

        // 业务查询方法
        public bool IsInProvince(string targetProvince)
        {
            return Province.Equals(targetProvince, StringComparison.OrdinalIgnoreCase);
        }

        public bool IsSameCity(Address other)
        {
            return City.Equals(other.City, StringComparison.OrdinalIgnoreCase);
        }

        public string GetFullAddress()
        {
            return $"{Province}{City}{District}{Street}";
        }

        // 值对象相等性比较
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Province;
            yield return City;
            yield return District ?? string.Empty;
            yield return Street;
            yield return ZipCode ?? string.Empty;
        }

        public override string ToString()
        {
            return $"Address(Province: {Province}, City: {City}, District: {District}, Street: {Street}, ZipCode: {ZipCode})";
        }
    }

    // 值对象基类
    public abstract class ValueObject
    {
        //ReferenceEquals 是 System.Object 类的静态方法，用于比较两个对象的引用是否指向同一个内存地址。
        // 那 ReferenceEquals 和 public override bool Equals(object obj) 的比较原理不太一样哈，
        // ReferenceEquals 比较的是，是不是同一个对象，Equals 比较的是对象里的值是否一样，有可能不是一个对象


        protected static bool EqualOperator(ValueObject left, ValueObject right)
        {
            // 第1步：只做 null 检查（使用 ReferenceEquals 避免递归）
            if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null))
            {
                return false;  // 一个为null，另一个不为null
            }

            // 第2步：两个都为null 或者 调用真正的值比较
            return ReferenceEquals(left, null) || left.Equals(right);
            //         ↑ 如果两个都为null，返回true      ↑ 否则调用值比较
        }
        protected static bool NotEqualOperator(ValueObject left, ValueObject right)
        {
            return !EqualOperator(left, right);
        }

        protected abstract IEnumerable<object> GetEqualityComponents();

        //SequenceEqual 是 LINQ 扩展方法，用于比较两个序列（集合）是否包含相同顺序的相同元素。

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var other = (ValueObject)obj;
            return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Select(x => x != null ? x.GetHashCode() : 0)
                .Aggregate((x, y) => x ^ y);
        }

        //使用案例
        //🎯 Distinct() 方法的工作原理
        //🎯 contains() 方法的工作原理

        public static bool operator ==(ValueObject left, ValueObject right)
        {
            return EqualOperator(left, right);
        }

        public static bool operator !=(ValueObject left, ValueObject right)
        {
            return NotEqualOperator(left, right);
        }

        //重载后（有 == 操作符重载）：
        private void UseCase()
        {
            var addr1 = new Address("北京", "北京", "海淀区", "中关村大街1号", "100080");
            var addr2 = new Address("北京", "北京", "海淀区", "中关村大街1号", "100080");

            Console.WriteLine(addr1 == addr2);           // True ✅（值比较）
            Console.WriteLine(addr1.Equals(addr2));      // True ✅（值比较）
            Console.WriteLine(addr1 != addr2);           // False ✅（值比较）

            var address1 = new Address("北京", "北京", "海淀区", "中关村大街1号", "100080");
            var address2 = new Address("北京", "北京", "海淀区", "中关村大街1号", "100080");
            var address3 = address1;  // 指向同一个对象
            var address4 = new Address("上海", "上海", "浦东新区", "陆家嘴", "200120");

            Console.WriteLine("=== ReferenceEquals 比较 ===");
            Console.WriteLine($"address1 vs address2: {ReferenceEquals(address1, address2)}");  // False
            Console.WriteLine($"address1 vs address3: {ReferenceEquals(address1, address3)}");  // True
            Console.WriteLine($"address1 vs address4: {ReferenceEquals(address1, address4)}");  // False

            Console.WriteLine("\n=== Equals 比较 ===");
            Console.WriteLine($"address1 vs address2: {address1.Equals(address2)}");  // True
            Console.WriteLine($"address1 vs address3: {address1.Equals(address3)}");  // True  
            Console.WriteLine($"address1 vs address4: {address1.Equals(address4)}");  // False
        }
    }
}