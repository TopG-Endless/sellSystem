using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
options.UseMySql("Server=localhost;Port=3306;Database=sell_system_data_base;User=root;Password=pass1234",
            new MySqlServerVersion(new Version(8, 0, 21))));

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => 
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Mi API con JWT", Version = "v1" });

    // Configuración para agregar el token JWT en Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Introduzca 'Bearer' [espacio] seguido de su token JWT."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

 
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {            
            ValidateIssuer = true, 
            ValidateAudience = true,
            ValidateLifetime = true, 
            ValidateIssuerSigningKey = true, 
            ValidIssuer = "yourdomain.com", 
            ValidAudience = "yourdomain.com",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("vainitaOMGclavelargaysegura_a234243423423awda"))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization(); 
app.UseMiddleware<MyFirstMDW>();

// Función para generar el JWT
string GenerateJwtToken()
{
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, "test"),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim("User","Mi usuario")
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("vainitaOMGclavelargaysegura_a234243423423awda"));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: "yourdomain.com",
        audience: "yourdomain.com",
        claims: claims,
        expires: DateTime.Now.AddMinutes(30),
        signingCredentials: creds);

    return new JwtSecurityTokenHandler().WriteToken(token);
}


// Endpoint de login para generar el JWT
app.MapPost("/login", (UserLogin login) =>
{
    if (login.Username == "test" && login.Password == "pass") // Validar credenciales
    {
        var token = GenerateJwtToken();
        return Results.Ok(new { token });
    }
    return Results.Unauthorized();
});


//  Endpoint to get all the companies
app.MapGet("/Companies", async (AppDbContext db) => 
{
    return await db.Companies.ToListAsync();
}).RequireAuthorization();

// EndPoint to get company by ID
app.MapGet("/Companies{id}", async (AppDbContext db, int id) => 
{
    var company = await db.Companies.FindAsync(id);

    if (company == null) {
        return Results.NotFound();
    }
        return Results.Ok(company);
});

// Endpoint to add a new company
app.MapPost("/Companies", async (AppDbContext db, Company company) =>
{
    if (string.IsNullOrWhiteSpace(company.CompanyName))
    {
        return Results.BadRequest("Company name is required.");
    }

    // Add company to the context
    db.Companies.Add(company);

    // Save the changes in the db
    await db.SaveChangesAsync();

    // Return company with generated ID
    return Results.Created($"/api/companies/{company.Id}", company);
});

    // Endpoint to modify company by ID
app.MapPut("/Companies", async (AppDbContext updatedDb, Company updatedCompany, int id) =>
{
    var company = await updatedDb.Companies.FindAsync(id);
    if (company == null)
            {
                return Results.NotFound();
            }

            company.CompanyName = updatedCompany.CompanyName;

            await updatedDb.SaveChangesAsync();    

            return Results.NoContent();
});

    //Endpoint to delete company by ID
app.MapDelete("/Companies{id}", async (AppDbContext db, int id) =>
{
    var company = await db.Companies.FindAsync(id);
    if (company is null) return Results.NotFound();

    var hasEmployees = await db.Employees.AnyAsync(e => e.CompanyId == id);
            if (hasEmployees)
            return Results.BadRequest("Cannot delete company because it has assigned employees.");

            db.Companies.Remove(company);

            await db.SaveChangesAsync();
            return Results.NoContent();

}
);

// Get all employees with their assigned company and orders
app.MapGet("/Employees", async (AppDbContext db) => 
{
    return await db.Employees.Include(e => e.Company)
                             .Include(e => e.Orders)
                             .ToListAsync();
}).RequireAuthorization();

// Get employee by ID with their company and orders
app.MapGet("/Employees/{id}", async (AppDbContext db, int id) =>
{
    var employee = await db.Employees.Include(e => e.Company)
                                     .Include(e => e.Orders)
                                     .FirstOrDefaultAsync(e => e.EmployeeId == id);
    return employee == null ? Results.NotFound() : Results.Ok(employee);
});

// Create a new employee and associate it with a company
app.MapPost("/Employees", async (AppDbContext db, Employee employee) =>
{
    // Check if the company exists
    var company = await db.Companies.FindAsync(employee.CompanyId);
    if (company == null)
        return Results.BadRequest("Invalid company ID.");

    // Add employee to the database
    db.Employees.Add(employee);
    await db.SaveChangesAsync();
    return Results.Created($"/Employees/{employee.EmployeeId}", employee);
});

// Update an existing employee by ID
app.MapPut("/Employees/{id}", async (AppDbContext db, Employee updatedEmployee, int id) =>
{
    var employee = await db.Employees.FindAsync(id);
    if (employee == null)
        return Results.NotFound();

    // Validate if the company exists
    var company = await db.Companies.FindAsync(updatedEmployee.CompanyId);
    if (company == null)
        return Results.BadRequest("Invalid company ID.");

    // Update employee details
    employee.EmployeeName = updatedEmployee.EmployeeName;
    employee.CompanyId = updatedEmployee.CompanyId;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Delete an employee by ID, ensuring they don't have active orders
app.MapDelete("/Employees/{id}", async (AppDbContext db, int id) =>
{
    var employee = await db.Employees.Include(e => e.Orders).FirstOrDefaultAsync(e => e.EmployeeId == id);
        if (employee == null)
        return Results.NotFound();

    // Check if the employee has any associated orders
    if (employee.Orders.Any())
        return Results.BadRequest("Cannot delete an employee with assigned orders.");

    db.Employees.Remove(employee);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Articles: Each article is associated with a company.
app.MapGet("/Articles", async (AppDbContext db) =>
{
    return await db.Articles.Include(a => a.Company).ToListAsync(); // Include company information
}).RequireAuthorization();

// id: Identifier of the article.
app.MapGet("/Articles/{id}", async (AppDbContext db, int id) =>
{
    var article = await db.Articles.Include(a => a.Company).FirstOrDefaultAsync(a => a.ArticleId == id); 
    return article == null ? Results.NotFound() : Results.Ok(article);
});

// Endpoint to add a new article
app.MapPost("/Articles", async (AppDbContext db, Article article) =>
{
    // Check if company exists before creating the article
    var company = await db.Companies.FindAsync(article.CompanyId);
    if (company == null)
        return Results.BadRequest("Invalid company ID.");

    db.Articles.Add(article);
    await db.SaveChangesAsync();
    return Results.Created($"/Articles/{article.ArticleId}", article);
});

app.MapPut("/Articles/{id}", async (AppDbContext db, Article updatedArticle, int id) =>
{
    var article = await db.Articles.FindAsync(id);
    if (article == null)
        return Results.NotFound();

    // Ensure company exists before updating the article
    var company = await db.Companies.FindAsync(updatedArticle.CompanyId);
    if (company == null)
        return Results.BadRequest("Invalid company ID.");

    article.ArticleName = updatedArticle.ArticleName;
    article.Price = updatedArticle.Price;
    article.CompanyId = updatedArticle.CompanyId;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/Articles/{id}", async (AppDbContext db, int id) =>
{
    var article = await db.Articles.FindAsync(id);
    if (article == null)
        return Results.NotFound();

    db.Articles.Remove(article);
    await db.SaveChangesAsync();
    return Results.NoContent();
});


// Orders: An order is assigned to an employee and can have multiple articles.
app.MapGet("/Orders", async (AppDbContext db) =>
{
    return await db.Orders.Include(o => o.Employee).Include(o => o.OrderDetails).ThenInclude(od => od.Article).ToListAsync(); 
}).RequireAuthorization();

// id: Identifier of the order.
app.MapGet("/Orders/{id}", async (AppDbContext db, int id) =>
{
    var order = await db.Orders.Include(o => o.Employee).Include(o => o.OrderDetails).ThenInclude(od => od.Article).FirstOrDefaultAsync(o => o.OrderId == id);
    return order == null ? Results.NotFound() : Results.Ok(order);
});

//Endpoint to add a new order.
app.MapPost("/Orders", async (AppDbContext db, Order order) =>
{
    // Validate if the employee exists
    var employee = await db.Employees.FindAsync(order.EmployeeId);
    if (employee == null)
        return Results.BadRequest("Invalid employee ID.");

    // Calculate total value of the order by summing up the prices of articles in order details
    order.TotalValue = order.OrderDetails.Sum(od => od.Article.Price * od.Quantity);

    db.Orders.Add(order);
    await db.SaveChangesAsync();
    return Results.Created($"/Orders/{order.OrderId}", order);
});

app.MapPut("/Orders/{id}", async (AppDbContext db, Order updatedOrder, int id) =>
{
    var order = await db.Orders.Include(o => o.OrderDetails).ThenInclude(od => od.Article).FirstOrDefaultAsync(o => o.OrderId == id);
    if (order == null)
        return Results.NotFound();

    // Validate if the employee exists
    var employee = await db.Employees.FindAsync(updatedOrder.EmployeeId);
    if (employee == null)
        return Results.BadRequest("Invalid employee ID.");

    order.OrderName = updatedOrder.OrderName;
    order.EmployeeId = updatedOrder.EmployeeId;
    order.Status = updatedOrder.Status;

    // Recalculate total value
    order.TotalValue = updatedOrder.OrderDetails.Sum(od => od.Article.Price * od.Quantity);

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/Orders/{id}", async (AppDbContext db, int id) =>
{
    var order = await db.Orders.FindAsync(id);
    if (order == null)
        return Results.NotFound();

    db.Orders.Remove(order);
    await db.SaveChangesAsync();
    return Results.NoContent();
});


// Invoice: Invoice generated from a completed order.
app.MapGet("/Invoices", async (AppDbContext db) =>
{
    return await db.Invoices.Include(i => i.Order).ToListAsync();
}).RequireAuthorization();

//Endpoint to get the invoice by ID
app.MapGet("/Invoices/{id}", async (AppDbContext db, int id) =>
{
    var invoice = await db.Invoices.Include(i => i.Order).FirstOrDefaultAsync(i => i.InvoiceId == id);
    return invoice == null ? Results.NotFound() : Results.Ok(invoice);
});

app.MapPost("/Invoices", async (AppDbContext db, Invoice invoice) =>
{
    // Validate if the order exists
    var order = await db.Orders.FindAsync(invoice.OrderId);
    if (order == null)
        return Results.BadRequest("Invalid order ID.");

    db.Invoices.Add(invoice);
    await db.SaveChangesAsync();
    return Results.Created($"/Invoices/{invoice.InvoiceId}", invoice);
});

app.MapPut("/Invoices/{id}", async (AppDbContext db, Invoice updatedInvoice, int id) =>
{
    var invoice = await db.Invoices.FindAsync(id);
    if (invoice == null)
        return Results.NotFound();

    // Validate if the order exists
    var order = await db.Orders.FindAsync(updatedInvoice.OrderId);
    if (order == null)
        return Results.BadRequest("Invalid order ID.");

    invoice.OrderId = updatedInvoice.OrderId;
    invoice.Status = updatedInvoice.Status;
    invoice.InvoiceETA = updatedInvoice.InvoiceETA;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/Invoices/{id}", async (AppDbContext db, int id) =>
{
    var invoice = await db.Invoices.FindAsync(id);
    if (invoice == null)
        return Results.NotFound();

    db.Invoices.Remove(invoice);
    await db.SaveChangesAsync();
    return Results.NoContent();
});


// Middleware personalizado
app.UseWhen(context => context.Request.Path.StartsWithSegments("/theone"), (appBuilder) =>
{
    appBuilder.Use(async (context, next) =>
    {                
        if (context.Request.Headers.ContainsKey("Key") && context.Request.Headers["Key"].ToString() == "vainitaOMG")
        {                        
            await next();
        }
        else
        {           
            context.Response.StatusCode = 400; 
            await context.Response.WriteAsync("Falta el header X-Custom-Header.");
        }
    });
});

app.Run();


public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }
    public DbSet<UserLogin> UserLogins { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Article> Articles { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Invoice> Invoices { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySql(
            "Server=localhost;Port=3306;Database=sell_system_data_base;User=root;Password=pass1234",
            new MySqlServerVersion(new Version(8, 0, 21)) // Cambia según tu versión de MySQL
        );
    }
}

public class UserLogin
{
    [Key]
    public required string Username { get; set; }
    public required string Password { get; set; }
}

public class Company
{
    [Key]
    public int Id { get; set; }

    public required string CompanyName { get; set; }

    // Navigation properties
    public required ICollection<Employee> Employees { get; set; }
    public required ICollection<Article> Articles { get; set; }
}
public class Employee
{
    [Key]
    public int EmployeeId { get; set; }

    public required string EmployeeName { get; set; }

    public int CompanyId { get; set; }  // Foreign Key
    public required Company Company { get; set; } // Navigation property

    // Navigation property
    public required ICollection<Order> Orders { get; set; }
}

public class Article
{
    [Key]
    public int ArticleId { get; set; }

    public required string ArticleName { get; set; }
    public decimal Price { get; set; }

    public int CompanyId { get; set; }  // Foreign Key
    public required Company Company { get; set; } // Navigation property

    // Navigation property
    public required ICollection<OrderDetail> OrderDetails { get; set; }
}

public class Order
{
    [Key]
    public int OrderId { get; set; }

    public required string OrderName { get; set; }
    public decimal TotalValue { get; set; }
    public string Status { get; set; } = "Pending";

    public int EmployeeId { get; set; }  // Foreign Key
    public required Employee Employee { get; set; } // Navigation property

    // Navigation properties
    public required ICollection<OrderDetail> OrderDetails { get; set; }
    public required Invoice Invoice { get; set; }
}

public class OrderDetail
{
    [Key]
    public int OrderDetailId { get; set; }

    public int ArticleId { get; set; }  // Foreign Key
    public required Article Article { get; set; } // Navigation property

    public int OrderId { get; set; }  // Foreign Key
    public required Order Order { get; set; } // Navigation property

    public int Quantity { get; set; }
}

public class Invoice
{
    [Key]
    public int InvoiceId { get; set; }

    public int OrderId { get; set; }  // Foreign Key
    public required Order Order { get; set; } // Navigation property

    public string Status { get; set; } = "Pending";
    public DateTime InvoiceETA { get; set; }
}