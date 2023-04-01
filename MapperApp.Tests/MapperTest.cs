
using MapperApp.Dto;
using MapperApp.Models;

namespace MapperApp.Tests;

public class MapperTest
{
    [Fact]
    public void Map_Person_To_PersonDto_Returns_EqualProperties_WithInnerObjects()
    {
        var person1 = new Person
        {
            FirstName = "Name1",
            LastName = "LastName1",
            Address = new Address
            {
                FlatName = "Name",
                HouseNumber = 42,
                StreetName = "street"
            }
        };
        
        var builder = new MapperConfigurationBuilder();
        builder.CreateMap<Person, PersonDto>()
            .ForMember(x => x.FullName, opt =>
                opt.Map(source => source.FirstName + " " + source.LastName));
           // .ForMember(x => x.Address, opt => opt.Ignore());

        var mapper = new Mapper(builder.Build(), ArraySegment<ICustomTypeMapper>.Empty);
        var dto = mapper.Map<PersonDto>(person1);

        

        Assert.Equal(dto.LastName, person1.LastName);
        Assert.Equal(dto.Address.FlatName, person1.Address.FlatName);
        Assert.Equal(dto.Address.StreetName, person1.Address.StreetName);
        Assert.Equal(dto.Address.HouseNumber, person1.Address.HouseNumber);
    }

    [Fact]
    public void Map_Band_To_BandDto_With_Collections_Returns_EqualAllMembers()
    {
        var frontMan = new Person() {FirstName = "Name", LastName = "LastName"};
        var band = new Band
        {
            FrontMan = frontMan,
            Name = "BandName",
            Members = new List<Person>
            {
                frontMan,
                new() {FirstName = "First", LastName = "Last"},
                new() {FirstName = "Second", LastName = "LastSecond"},
                new() {FirstName = "Thirst", LastName = "LastThirst"},
            }
        };

        var builder = new MapperConfigurationBuilder();
        builder.CreateMap<Person, PersonDto>()
            .ForMember(x => x.FullName, x => 
                x.Map(y => y.FirstName + " " + y.LastName));
        builder.CreateMap<Band, BandDto>();

        var configuration = builder.Build();
        var mapper = new Mapper(configuration, new []{new ListToArrayMapper()});
        var bandDto = mapper.Map<BandDto>(band);
        
        Assert.Equal(frontMan.FirstName, bandDto.FrontMan.FirstName);
        Assert.Equal(frontMan.LastName, bandDto.FrontMan.LastName);
        
        Assert.Collection(bandDto.Members, 
            el1 =>
        {
            Assert.Equal("Name", el1.FirstName);
            Assert.Equal("LastName", el1.LastName);
        }, el2 =>
        {
            Assert.Equal("First", el2.FirstName);
            Assert.Equal("Last", el2.LastName);
        }, el3 => 
        {
            Assert.Equal("Second", el3.FirstName);
            Assert.Equal("LastSecond", el3.LastName);
        }, el4 => 
        {
            Assert.Equal("Thirst", el4.FirstName);
            Assert.Equal("LastThirst", el4.LastName); 
        });
    }

    [Fact]
    public void Map_Book_Class_To_BookDto_Record_Returns_EqualProperties()
    {
        var book = new Book
        {
            Author = "Terry Pratchett",
            Name = "Guards! Guards!",
            Text = "Sir Samuel...",
            PublishHouse = "Buamaga"
        };
        var builder = new MapperConfigurationBuilder();
        builder.CreateMap<Book, BookDto>();
        var mapperConfig = builder.Build();
        var mapper = new Mapper(mapperConfig, ArraySegment<ICustomTypeMapper>.Empty);
        var dto = mapper.Map<BookDto>(book);
        
        Assert.Equal(book.Author, dto.Author);
        Assert.Equal(book.Name, dto.Name);
        Assert.Equal(book.Text, dto.Text);
        Assert.Equal(book.PublishHouse, dto.PublishHouse);
    }
}