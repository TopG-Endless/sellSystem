using System.ComponentModel.DataAnnotations;


public class CreateEmployeeDto
{
    public required string EmployeeName { get; set; }
    public int CompanyId { get; set; }  // Foreign Key
}
 
public class UpdateEmployeeDto
{
    public int EmployeeId { get; set; }
    public required string EmployeeName { get; set; }
    public int CompanyId { get; set; }  // Foreign Key
}



