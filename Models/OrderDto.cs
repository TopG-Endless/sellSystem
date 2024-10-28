// DTO for creating a new order
public class CreateOrderDto
{
    public required string OrderName { get; set; }
    public decimal TotalValue { get; set; }
    public string Status { get; set; } = "Pending";
    public int EmployeeId { get; set; }  // Foreign Key
    public List<CreateOrderDetailDto> OrderDetail { get; set; } = new();
}



// DTO for reading order details
public class ReadOrderDto
{
    public int OrderId { get; set; }
    public required string OrderName { get; set; }
    public decimal TotalValue { get; set; }
    public string Status { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; }  // Related Employee's name
    public List<ReadOrderDetailDto> OrderDetails { get; set; } = new();
}



// DTO for updating an order
public class UpdateOrderDto
{
    public required string OrderName { get; set; }
    public string Status { get; set; } = "Pending";
    public int EmployeeId { get; set; }
    public List<UpdateOrderDetailDto> OrderDetail { get; set; } = new();
}


