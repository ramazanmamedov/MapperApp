namespace MapperApp.Models;

public class Band
{
    public string Name { get; set; }
    public Person FrontMan { get; set; }
    public IEnumerable<Person> Members { get; set; }
}