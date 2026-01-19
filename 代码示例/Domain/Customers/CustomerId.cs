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

        //  隐式转换到Guid implicit operator Guid(CustomerId id)
        public static implicit operator Guid(CustomerId id) => id.Value;

        // 隐式转换从Guid implicit operator CustomerId(Guid value)
        public static implicit operator CustomerId(Guid value) => From(value);

        // 隐式转换从string implicit operator CustomerId(Guid value)
        public static implicit operator CustomerId(string value) => From(value);

        // 隐式转换到字符串 implicit operator string(CustomerId id)
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


//场景1：数据库存储和检索（Entity Framework Core）
// 实体类定义
public class Customer
{
    public CustomerId Id { get; private set; } // 使用强类型ID
    public string Name { get; set; }
    public string Email { get; set; }
}

// DbContext配置
public class AppDbContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            // 隐式转换到Guid：Entity Framework Core会自动将CustomerId转为Guid存储
            entity.Property(e => e.Id)
                  .HasConversion(
                      id => id.Value,          // 存储时：CustomerId → Guid
                      value => new CustomerId(value)) // 读取时：Guid → CustomerId
                  .HasColumnName("CustomerId");
        });
    }
}

// 使用示例
public class CustomerRepository
{
    private readonly AppDbContext _dbContext;

    public async Task<Customer> GetCustomerAsync(CustomerId customerId)
    {
        // 场景1：查询时，customerId自动转换为Guid与数据库比较
        // 由于有 implicit operator Guid，可以直接用customerId作为Guid参数
        return await _dbContext.Customers
            .FirstOrDefaultAsync(c => c.Id == customerId);
    }

    public async Task<List<Customer>> GetCustomersAsync(List<CustomerId> customerIds)
    {
        // 场景2：在Contains查询中，customerIds列表自动转换为Guid列表
        // 隐式转换让代码更简洁
        return await _dbContext.Customers
            .Where(c => customerIds.Contains(c.Id))
            .ToListAsync();
    }
}

//场景2：API接口（Controller层）
[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly CustomerService _customerService;

    // GET api/customers/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCustomer(string id)
    {
        // 场景1：从路由参数获取字符串，隐式转换为CustomerId
        // 因为定义了 implicit operator CustomerId(string value)
        // 但实际上你用的是From方法，所以这里演示的是 string → CustomerId 的转换
        try
        {
            CustomerId customerId = id; // 这会调用 CustomerId.From(id) 或类似转换

            var customer = await _customerService.GetCustomerAsync(customerId);
            return Ok(customer);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // POST api/customers
    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request)
    {
        // 场景2：在DTO中直接使用CustomerId类型
        var customerId = CustomerId.New(); // 生成新ID
        var customer = new Customer(customerId, request.Name, request.Email);

        await _customerService.CreateCustomerAsync(customer);

        // 返回时，customerId会自动转换为字符串
        return CreatedAtAction(nameof(GetCustomer),
            new { id = customerId }, // 隐式转换为string
            customer);
    }

    // PUT api/customers/{id}/orders
    [HttpPut("{id}/orders")]
    public async Task<IActionResult> AddOrder(string id, [FromBody] AddOrderRequest request)
    {
        // 场景3：在查询参数中使用
        CustomerId customerId = id; // 字符串隐式转换

        // 场景4：在方法调用中自动转换
        var orderId = Guid.NewGuid(); // 假设从其他地方获取的Guid
        await _customerService.AddOrderAsync(
            customerId,          // CustomerId类型
            orderId);            // Guid类型，自动转换为CustomerId

        return Ok();
    }
}

//场景3：领域服务和业务逻辑层
public class CustomerService
{
    private readonly ICustomerRepository _repository;
    private readonly IEmailService _emailService;

    public async Task<Customer> GetCustomerAsync(CustomerId customerId)
    {
        // 场景1：在领域服务中直接使用CustomerId类型
        var customer = await _repository.GetByIdAsync(customerId);

        if (customer == null)
        {
            // 场景2：在异常消息中，customerId自动转换为字符串
            throw new CustomerNotFoundException($"客户 {customerId} 不存在");
        }

        return customer;
    }

    public async Task SendWelcomeEmailAsync(CustomerId customerId)
    {
        var customer = await GetCustomerAsync(customerId);

        // 场景3：在发送邮件时，customerId自动转换为字符串用于日志或链接
        var welcomeLink = $"https://our-app.com/welcome?customerId={customerId}";

        await _emailService.SendAsync(
            to: customer.Email,
            subject: "欢迎注册",
            body: $"<p>点击链接激活账户: <a href='{welcomeLink}'>激活</a></p>"
        );

        // 日志记录
        _logger.LogInformation($"已发送欢迎邮件给客户: {customerId}");
    }

    public async Task TransferCustomerAsync(
        CustomerId sourceCustomerId,
        CustomerId targetCustomerId,
        decimal amount)
    {
        // 场景4：在复杂业务逻辑中直接比较两个CustomerId
        if (sourceCustomerId == targetCustomerId)
        {
            throw new InvalidOperationException("不能转账给自己");
        }

        // 场景5：在存储过程中可能需要Guid
        var sourceGuid = (Guid)sourceCustomerId; // 显式转换，也可以隐式转换
        var targetGuid = targetCustomerId;       // 隐式转换到Guid

        await _repository.TransferFundsAsync(sourceGuid, targetGuid, amount);
    }
}