using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Common
{
    public abstract class Entity
    {
        private List<IDomainEvent> _domainEvents;

        [NotMapped] // 确保不会被EF Core持久化
        public IReadOnlyCollection<IDomainEvent> DomainEvents =>
            _domainEvents?.AsReadOnly() ?? new List<IDomainEvent>().AsReadOnly();
        // 等价于：
        // public IReadOnlyCollection<IDomainEvent> DomainEvents
        // {
        //     get
        //     {
        //         if (_domainEvents != null)
        //         {
        //             return _domainEvents.AsReadOnly();
        //         }
        //         else
        //         {
        //             return new List<IDomainEvent>().AsReadOnly();
        //         }
        //     }
        // }
        public void AddDomainEvent(IDomainEvent eventItem)
        {
            _domainEvents ??= new List<IDomainEvent>();
            _domainEvents.Add(eventItem);
        }
        // //??= 可以理解为："如果左边是null，就把右边的值赋给左边"
        // // 假设我们有一个类
        // public class Person
        // {
        //     private List<string> _hobbies;

        //     public List<string> Hobbies
        //     {
        //         get
        //         {
        //             // 延迟初始化：第一次访问时才创建列表
        //             _hobbies ??= new List<string>();
        //             return _hobbies;
        //         }
        //     }
        // }


        // // 这等价于：
        // public List<string> Hobbies
        // {
        //     get
        //     {
        //         if (_hobbies == null)
        //         {
        //             _hobbies = new List<string>();
        //         }
        //         return _hobbies;
        //     }
        // }
        public void RemoveDomainEvent(IDomainEvent eventItem)
        {
            _domainEvents?.Remove(eventItem);
        }

        public void ClearDomainEvents()
        {
            _domainEvents?.Clear();
        }
    }
}