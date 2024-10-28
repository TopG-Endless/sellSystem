using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class OrderDetailsController : ControllerBase
{
    private readonly AppDbContext _context;

    public OrderDetailsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/OrderDetails
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReadOrderDetailDto>>> GetOrderDetails()
    {
        var orderDetails = await _context.OrderDetail
            .Include(od => od.Article)
            .Include(od => od.Order)
            .Select(od => new ReadOrderDetailDto
            {
                OrderDetailId = od.OrderDetailId,
                ArticleId = od.ArticleId,
                ArticleName = od.Article.ArticleName,
                Price = od.Article.Price,
                Quantity = od.Quantity
            })
            .ToListAsync();

        return Ok(orderDetails);
    }

    // GET: api/OrderDetails/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ReadOrderDetailDto>> GetOrderDetail(int id)
    {
        var orderDetail = await _context.OrderDetail
            .Include(od => od.Article)
            .Include(od => od.Order)
            .Select(od => new ReadOrderDetailDto
            {
                OrderDetailId = od.OrderDetailId,
                ArticleId = od.ArticleId,
                ArticleName = od.Article.ArticleName,
                Price = od.Article.Price,
                Quantity = od.Quantity
            })
            .FirstOrDefaultAsync(od => od.OrderDetailId == id);

        if (orderDetail == null)
        {
            return NotFound();
        }

        return Ok(orderDetail);
    }

    // POST: api/OrderDetails
    [HttpPost]
    public async Task<ActionResult<ReadOrderDetailDto>> PostOrderDetail(CreateOrderDetailDto createOrderDetailDto)
    {
        var article = await _context.Articles.FindAsync(createOrderDetailDto.ArticleId);
        var order = await _context.Orders.FindAsync(createOrderDetailDto.OrderId);

        if (article == null || order == null)
        {
            return BadRequest("Invalid Article or Order ID.");
        }

        var orderDetail = new OrderDetail
        {
            ArticleId = createOrderDetailDto.ArticleId,
            OrderId = createOrderDetailDto.OrderId,
            Quantity = createOrderDetailDto.Quantity
        };

        _context.OrderDetail.Add(orderDetail);
        await _context.SaveChangesAsync();

        var readOrderDetail = new ReadOrderDetailDto
        {
            OrderDetailId = orderDetail.OrderDetailId,
            ArticleId = orderDetail.ArticleId,
            ArticleName = article.ArticleName,
            Price = article.Price,
            Quantity = orderDetail.Quantity
        };

        return CreatedAtAction("GetOrderDetail", new { id = orderDetail.OrderDetailId }, readOrderDetail);
    }

    // PUT: api/OrderDetails/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutOrderDetail(int id, UpdateOrderDetailDto updateOrderDetailDto)
    {
        if (id != updateOrderDetailDto.OrderDetailId)
        {
            return BadRequest();
        }

        var orderDetail = await _context.OrderDetail.FindAsync(id);
        if (orderDetail == null) return NotFound();

        var article = await _context.Articles.FindAsync(updateOrderDetailDto.ArticleId);
        var order = await _context.Orders.FindAsync(updateOrderDetailDto.OrderId);

        if (article == null || order == null)
        {
            return BadRequest("Invalid Article or Order ID.");
        }

        orderDetail.ArticleId = updateOrderDetailDto.ArticleId;
        orderDetail.OrderId = updateOrderDetailDto.OrderId;
        orderDetail.Quantity = updateOrderDetailDto.Quantity;

        _context.Entry(orderDetail).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!OrderDetailExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/OrderDetails/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrderDetail(int id)
    {
        var orderDetail = await _context.OrderDetail.FindAsync(id);
        if (orderDetail == null)
        {
            return NotFound();
        }

        _context.OrderDetail.Remove(orderDetail);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool OrderDetailExists(int id)
    {
        return _context.OrderDetail.Any(e => e.OrderDetailId == id);
    }
}
