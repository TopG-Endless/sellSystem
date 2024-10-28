using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class InvoiceController : ControllerBase
{
    private readonly AppDbContext _context;

    public InvoiceController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllInvoices()
    {
        var invoices = await _context.Invoice
            .Include(i => i.Order)
            .Select(i => new ReadInvoiceDto
            {
                InvoiceId = i.InvoiceId,
                OrderId = i.OrderId,
                Status = i.Status,
                InvoiceETA = i.InvoiceETA
            })
            .ToListAsync();
        
        return Ok(invoices);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetInvoiceById(int id)
    {
        var invoice = await _context.Invoice
            .Include(i => i.Order)
            .Select(i => new ReadInvoiceDto
            {
                InvoiceId = i.InvoiceId,
                OrderId = i.OrderId,
                Status = i.Status,
                InvoiceETA = i.InvoiceETA
            })
            .FirstOrDefaultAsync(i => i.InvoiceId == id);
        
        return invoice == null ? NotFound() : Ok(invoice);
    }

    [HttpPost]
    public async Task<IActionResult> AddInvoice(CreateInvoiceDto createInvoiceDto)
    {
        // Validate if the order exists
        var order = await _context.Orders.FindAsync(createInvoiceDto.OrderId);
        if (order == null)
            return BadRequest("Invalid order ID.");

        var invoice = new Invoice
        {
            OrderId = createInvoiceDto.OrderId,
            InvoiceETA = createInvoiceDto.InvoiceETA,
            Status = "Pending" // Default status, modify as necessary
        };

        _context.Invoice.Add(invoice);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetInvoiceById), new { id = invoice.InvoiceId }, invoice);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateInvoice(int id, UpdateInvoiceDto updateInvoiceDto)
    {
        var invoice = await _context.Invoice.FindAsync(id);
        if (invoice == null)
            return NotFound();

        // Validate if the order exists
        var order = await _context.Orders.FindAsync(updateInvoiceDto.OrderId);
        if (order == null)
            return BadRequest("Invalid order ID.");

        invoice.OrderId = updateInvoiceDto.OrderId;
        invoice.Status = updateInvoiceDto.Status;
        invoice.InvoiceETA = updateInvoiceDto.InvoiceETA;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteInvoice(int id)
    {
        var invoice = await _context.Invoice.FindAsync(id);
        if (invoice == null)
            return NotFound();

        _context.Invoice.Remove(invoice);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
