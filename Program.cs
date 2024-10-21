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


//  Endpoint para generar todas las companias
app.MapGet("/Companies", async (AppDbContext db) => 
{
    return await db.Companies.ToListAsync();
}).RequireAuthorization();

// EndPoint para generar compania por id
app.MapGet("/Companies{id}", async (AppDbContext db, int id) => 
{
    var company = await db.Companies.FindAsync(id);

    if (company == null) {
        return Results.NotFound();
    }
        return Results.Ok(company);
});

// Endpoint para agregar compania
app.MapPost("/Companies", async (AppDbContext db, Company company) =>
{
    if (string.IsNullOrWhiteSpace(company.CompanyName))
    {
        return Results.BadRequest("Company name is required.");
    }

    // Agregar la compañía al contexto
    db.Companies.Add(company);

    // Guardar los cambios en la base de datos
    await db.SaveChangesAsync();

    // Retornar la compañía junto con su ID generado
    return Results.Created($"/api/companies/{company.Id}", company);
});

    // Endpoint para modificar compania por su ID
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

    //Endpoint para eliminar compania por su ID
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
    public int TotalValue { get; set; }
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


