namespace MapperApp.Dto;

public class PersonDto
{
    public string FullName { get; set; }
    public string LastName { get; set; }
    public AddressDto Address { get; set; }
    public string FirstName { get; set; }
}