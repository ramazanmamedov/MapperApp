namespace MapperApp.Dto;

public class BandDto
{
    public string Name { get; set; }
    public PersonDto FrontMan { get; set; }
    public PersonDto[] Members { get; set; }
}