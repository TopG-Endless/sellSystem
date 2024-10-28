using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class CompanyController : ControllerBase
{

private readonly AppDbContext _Context;

        public CompanyController(AppDbContext dbContext)
        {
            _Context = dbContext;
        }

   
   [HttpGet]
public async Task<IActionResult> GetAllCompanies()
{
    var companies = await _Context.Companies
        .Include(c => c.Employees)
        .Include(c => c.Articles)
        .ToListAsync();

    var companyDtos = companies.Select(c => new CompanyDto
    {
        Id = c.Id,
        CompanyName = c.CompanyName,
        Employees = c.Employees.Select(e => new EmployeeDto
        {
            EmployeeId = e.EmployeeId,
            EmployeeName = e.EmployeeName
        }).ToList(),
        Articles = c.Articles.Select(a => new ArticleDto
        {
            ArticleId = a.ArticleId,
            ArticleName = a.ArticleName,
            Price = a.Price
        }).ToList()
    }).ToList();

    return Ok(companyDtos);
}

[HttpGet("{id}")]
public async Task<IActionResult> GetCompanyById(int id)
{
    var company = await _Context.Companies
        .Include(c => c.Employees)
        .Include(c => c.Articles)
        .FirstOrDefaultAsync(c => c.Id == id);

    if (company == null) return NotFound();

    var companyDto = new CompanyDto
    {
        Id = company.Id,
        CompanyName = company.CompanyName,
        Employees = company.Employees.Select(e => new EmployeeDto
        {
            EmployeeId = e.EmployeeId,
            EmployeeName = e.EmployeeName
        }).ToList(),
        Articles = company.Articles.Select(a => new ArticleDto
        {
            ArticleId = a.ArticleId,
            ArticleName = a.ArticleName,
            Price = a.Price
        }).ToList()
    };

    return Ok(companyDto);
}

[HttpPost]
public async Task<IActionResult> AddCompany(CreateCompanyDto createCompanyDto)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }

    var company = new Company
    {
        CompanyName = createCompanyDto.CompanyName,
        Employees = new List<Employee>(), // Puedes agregar empleados aquí si lo deseas
        Articles = new List<Article>() // Puedes agregar artículos aquí si lo deseas
    };

    _Context.Companies.Add(company);
    await _Context.SaveChangesAsync();
    return CreatedAtAction(nameof(GetCompanyById), new { id = company.Id }, company);
}

[HttpPut("{id}")]
public async Task<IActionResult> UpdateCompany(int id, UpdateCompanyDto updateCompanyDto)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }

    var company = await _Context.Companies.FindAsync(id);
    if (company == null) return NotFound();

    company.CompanyName = updateCompanyDto.CompanyName;

    // Aquí puedes actualizar empleados y artículos si es necesario

    await _Context.SaveChangesAsync();
    return NoContent();
}

[HttpDelete("{id}")]
public async Task<IActionResult> DeleteCompany(int id)
{
    var company = await _Context.Companies.FindAsync(id);
    if (company == null) return NotFound();

    var hasEmployees = await _Context.Employees.AnyAsync(e => e.CompanyId == id);
    if (hasEmployees)
    {
        return BadRequest("Cannot delete company because it has assigned employees.");
    }

    _Context.Companies.Remove(company);
    await _Context.SaveChangesAsync();
    return NoContent();
}

}