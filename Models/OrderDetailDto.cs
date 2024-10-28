public class ReadOrderDetailDto
{
    public int OrderDetailId { get; set; }
    public int ArticleId { get; set; }
    public string ArticleName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public class CreateOrderDetailDto
{
    public int ArticleId { get; set; }
    public int OrderId { get; set; }
    public int Quantity { get; set; }
}

public class UpdateOrderDetailDto
{
    public int OrderDetailId { get; set; }
    public int ArticleId { get; set; }
    public int OrderId { get; set; }
    public int Quantity { get; set; }
}
