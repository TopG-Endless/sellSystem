using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly AppDbContext _context;

    public OrderController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllOrders()
    {
        var orders = await _context.Orders
            .Include(o => o.Employee)
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Article)
            .Select(o => new ReadOrderDto
            {
                OrderId = o.OrderId,
                OrderName = o.OrderName,
                TotalValue = o.TotalValue,
                Status = o.Status,
                EmployeeId = o.EmployeeId,
                EmployeeName = o.Employee.EmployeeName,
                OrderDetails = o.OrderDetails.Select(od => new ReadOrderDetailDto
                {
                    ArticleId = od.ArticleId,
                    ArticleName = od.Article.ArticleName,
                    Price = od.Article.Price,
                    Quantity = od.Quantity
                }).ToList()
            })
            .ToListAsync();
        
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Employee)
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Article)
            .Select(o => new ReadOrderDto
            {
                OrderId = o.OrderId,
                OrderName = o.OrderName,
                TotalValue = o.TotalValue,
                Status = o.Status,
                EmployeeId = o.EmployeeId,
                EmployeeName = o.Employee.EmployeeName,
                OrderDetails = o.OrderDetails.Select(od => new ReadOrderDetailDto
                {
                    ArticleId = od.ArticleId,
                    ArticleName = od.Article.ArticleName,
                    Price = od.Article.Price,
                    Quantity = od.Quantity
                }).ToList()
            })
            .FirstOrDefaultAsync(o => o.OrderId == id);

        if (order == null) return NotFound();
        return Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> AddOrder(CreateOrderDto createOrderDto)
    {
        var employee = await _context.Employees.FindAsync(createOrderDto.EmployeeId);
        if (employee == null)
        {
            return BadRequest("Invalid employee ID.");
        }

        var order = new Order
        {
            OrderName = createOrderDto.OrderName,
            EmployeeId = createOrderDto.EmployeeId,
            Status = createOrderDto.Status,
            OrderDetails = createOrderDto.OrderDetail.Select(od => new OrderDetail
            {
                ArticleId = od.ArticleId,
                Quantity = od.Quantity
            }).ToList()
        };

        order.TotalValue = order.OrderDetails.Sum(od => od.Article.Price * od.Quantity);

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetOrderById), new { id = order.OrderId }, order);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrder(int id, UpdateOrderDto updateOrderDto)
    {
        var order = await _context.Orders
            .Include(o => o.OrderDetails)
            .FirstOrDefaultAsync(o => o.OrderId == id);

        if (order == null) return NotFound();

        var employee = await _context.Employees.FindAsync(updateOrderDto.EmployeeId);
        if (employee == null) return BadRequest("Invalid employee ID.");

        order.OrderName = updateOrderDto.OrderName;
        order.EmployeeId = updateOrderDto.EmployeeId;
        order.Status = updateOrderDto.Status;

        // Updating OrderDetails
        _context.OrderDetail.RemoveRange(order.OrderDetails);
        order.OrderDetails = updateOrderDto.OrderDetail.Select(od => new OrderDetail
        {
            ArticleId = od.ArticleId,
            Quantity = od.Quantity
        }).ToList();

        order.TotalValue = order.OrderDetails.Sum(od => od.Article.Price * od.Quantity);

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var order = await _context.Orders
            .Include(o => o.OrderDetails)
            .FirstOrDefaultAsync(o => o.OrderId == id);

        if (order == null) return NotFound();

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
