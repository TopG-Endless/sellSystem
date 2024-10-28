using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
public class CompanyDto
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Company name is required.")]
    public required string CompanyName { get; set; }

    // Propiedades de navegación
    // Mark Employees with JsonIgnore
    [JsonIgnore]
    public ICollection<EmployeeDto> Employees { get; set; } = new List<EmployeeDto>();
    public ICollection<ArticleDto> Articles { get; set; } = new List<ArticleDto>();
}

public class CreateCompanyDto
{
    [Required(ErrorMessage = "Company name is required.")]
    public required string CompanyName { get; set; }

    // Puedes incluir otras propiedades si deseas permitir su creación
}

public class UpdateCompanyDto
{
    [Required(ErrorMessage = "Company name is required.")]
    public required string CompanyName { get; set; }

    // Incluye otros campos que quieras actualizar
}

public class EmployeeDto
{
    [Key]
    public int EmployeeId { get; set; }
    public required string EmployeeName { get; set; }
    public int CompanyId { get; set; }  // Foreign Key

     // Mark Employees with JsonIgnore
    [JsonIgnore]
    public CompanyDto Company { get; set; }
    
}

    

// DTO para Article
public class ArticleDto
{
    public int ArticleId { get; set; }

    [Required(ErrorMessage = "Article name is required.")]
    public required string ArticleName { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
    public decimal Price { get; set; }
}
