//En el programa.cs
//app.UseeMiddleware<MyFirstMDW>();
public class MyFirstMDW
{
    private readonly RequestDelegate _next;

    public MyFirstMDW(RequestDelegate next)
    {
        _next = next;
    }

     public async Task Invoke(HttpContext context)
    {
        Console.WriteLine("------------>Aqui Escribi<------------");
        // Pasa el request al siguiente middleware en la cadena
        await _next(context);
    }
}
