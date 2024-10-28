using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class EmployeeController : ControllerBase
{
    private readonly AppDbContext _context;

    public EmployeeController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Employee
    [HttpGet]
    public async Task<IActionResult> GetAllEmployees()
    {
        var employees = await _context.Employees
            .Include(e => e.Company) // Include the Company for navigation property
            .ToListAsync();

        var employeeDtos = employees.Select(e => new EmployeeDto
        {
            EmployeeId = e.EmployeeId,
            EmployeeName = e.EmployeeName,
            CompanyId = e.CompanyId,
            Company = new CompanyDto
            {
                Id = e.Company.Id,
                CompanyName = e.Company.CompanyName
            }
        }).ToList();

        return Ok(employeeDtos);
    }

    // GET: api/Employee/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEmployeeById(int id)
    {
        var employee = await _context.Employees
            .Include(e => e.Company) // Include the Company for navigation property
            .FirstOrDefaultAsync(e => e.EmployeeId == id);

        if (employee == null) return NotFound();

        var employeeDto = new EmployeeDto
        {
            EmployeeId = employee.EmployeeId,
            EmployeeName = employee.EmployeeName,
            CompanyId = employee.CompanyId,
            Company = new CompanyDto
            {
                Id = employee.Company.Id,
                CompanyName = employee.Company.CompanyName
            }
        };

        return Ok(employeeDto);
    }

    // POST: api/Employee
    [HttpPost]
    public async Task<IActionResult> AddEmployee(CreateEmployeeDto createEmployeeDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var company = await _context.Companies.FindAsync(createEmployeeDto.CompanyId);
        if (company == null)
        {
            return BadRequest("Invalid company ID.");
        }

        var employee = new Employee
        {
            EmployeeName = createEmployeeDto.EmployeeName,
            CompanyId = createEmployeeDto.CompanyId,
            Company = company // Set the Company navigation property
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetEmployeeById), new { id = employee.EmployeeId }, employee);
    }

    // PUT: api/Employee/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEmployee(int id, UpdateEmployeeDto updateEmployeeDto)
    {
        if (id != updateEmployeeDto.EmployeeId)
        {
            return BadRequest("Employee ID mismatch.");
        }

        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return NotFound();

        var company = await _context.Companies.FindAsync(updateEmployeeDto.CompanyId);
        if (company == null) return BadRequest("Invalid company ID.");

        employee.EmployeeName = updateEmployeeDto.EmployeeName;
        employee.CompanyId = updateEmployeeDto.CompanyId;
        employee.Company = company; // Set the Company navigation property

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/Employee/{id}
    [HttpDelete("{id}")]
public async Task<IActionResult> DeleteEmployee(int id)
{
    var employee = await _context.Employees
        .FirstOrDefaultAsync(e => e.EmployeeId == id);

    if (employee == null) return NotFound();

    _context.Employees.Remove(employee);
    await _context.SaveChangesAsync();
    return NoContent();
}

}
