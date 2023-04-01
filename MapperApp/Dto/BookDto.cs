namespace MapperApp.Dto;

public record BookDto(string Author, string Name, string Text, int PageCount = 0)
{
    public string PublishHouse { get; set; }
}