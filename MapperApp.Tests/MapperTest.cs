
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

        var mapper = new Mapper(builder.Build());
        var dto = mapper.Map<PersonDto>(person1);

        

        Assert.Equal(dto.LastName, person1.LastName);
        Assert.Equal(dto.Address.FlatName, person1.Address.FlatName);
        Assert.Equal(dto.Address.StreetName, person1.Address.StreetName);
        Assert.Equal(dto.Address.HouseNumber, person1.Address.HouseNumber);
    }
}