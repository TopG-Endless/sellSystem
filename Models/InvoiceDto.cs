public class ReadInvoiceDto
{
    public int InvoiceId { get; set; }
    public int OrderId { get; set; }
    public string Status { get; set; }
    public DateTime InvoiceETA { get; set; }
}

public class CreateInvoiceDto
{
    public int OrderId { get; set; }
    public DateTime InvoiceETA { get; set; }
}

public class UpdateInvoiceDto
{
    public int OrderId { get; set; }
    public string Status { get; set; }
    public DateTime InvoiceETA { get; set; }
}
