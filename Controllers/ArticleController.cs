using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class ArticleController : ControllerBase
{
    private readonly AppDbContext _context;

    public ArticleController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Article
    [HttpGet]
    public async Task<IActionResult> GetAllArticles()
    {
        var articles = await _context.Articles
            .Include(a => a.Company)
            .Select(a => new ReadArticleDto
            {
                ArticleId = a.ArticleId,
                ArticleName = a.ArticleName,
                Price = a.Price,
                CompanyId = a.CompanyId,
                Company = a.Company.CompanyName
                
            })
            .ToListAsync();

        return Ok(articles);
    }

    // GET: api/Article/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetArticleById(int id)
    {
        var article = await _context.Articles
            .Include(a => a.Company)
            .FirstOrDefaultAsync(a => a.ArticleId == id);

        if (article == null)
            return NotFound();

        var articleDto = new ReadArticleDto
        {
            ArticleId = article.ArticleId,
            ArticleName = article.ArticleName,
            Price = article.Price,
            CompanyId = article.CompanyId,
            Company = article.Company.CompanyName
        };

        return Ok(articleDto);
    }

    // POST: api/Article
    [HttpPost]
    public async Task<IActionResult> AddArticle(CreateArticleDto articleDto)
    {
        var company = await _context.Companies.FindAsync(articleDto.CompanyId);
        if (company == null)
        {
            return BadRequest("Invalid company ID.");
        }

        var article = new Article
        {
            ArticleName = articleDto.ArticleName,
            Price = articleDto.Price,
            CompanyId = articleDto.CompanyId,
            Company = company
        };

        _context.Articles.Add(article);
        await _context.SaveChangesAsync();

        var readArticleDto = new ReadArticleDto
        {
            ArticleId = article.ArticleId,
            ArticleName = article.ArticleName,
            Price = article.Price,
            CompanyId = article.CompanyId,
            Company = article.Company.CompanyName
        };

        return CreatedAtAction(nameof(GetArticleById), new { id = article.ArticleId }, readArticleDto);
    }

    // PUT: api/Article/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateArticle(int id, UpdateArticleDto articleDto)
    {
        var article = await _context.Articles.FindAsync(id);
        if (article == null)
        {
            return NotFound();
        }

        var company = await _context.Companies.FindAsync(articleDto.CompanyId);
        if (company == null)
        {
            return BadRequest("Invalid company ID.");
        }

        article.ArticleName = articleDto.ArticleName;
        article.Price = articleDto.Price;
        article.CompanyId = articleDto.CompanyId;
        article.Company = company;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Article/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteArticle(int id)
    {
        var article = await _context.Articles.FindAsync(id);
        if (article == null)
        {
            return NotFound();
        }

        _context.Articles.Remove(article);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
