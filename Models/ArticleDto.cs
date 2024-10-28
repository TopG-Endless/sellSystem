    public class CreateArticleDto
{
    public required string ArticleName { get; set; }
    public decimal Price { get; set; }
    public int CompanyId { get; set; }  // Foreign Key
}

public class UpdateArticleDto
{
    public int ArticleId { get; set; }  // ID is needed to identify the article
    public required string ArticleName { get; set; }
    public decimal Price { get; set; }
    public int CompanyId { get; set; }  // Foreign Key
}

public class ReadArticleDto
{
    public int ArticleId { get; set; }
    public required string ArticleName { get; set; }
    public decimal Price { get; set; }

    // Foreign Key
    public int CompanyId { get; set; }
    public string Company { get; set; }

}