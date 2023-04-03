using MapperApp.Dto;
using MapperApp.Models;

namespace MapperApp.Tests;

public class ExpressionBasedTest
{
    [Fact]
    public void Map_Person_To_PersonDto_Returns_EqualProperties_WithInnerObjects()
    {
        var person1 = new Person
        {
            FirstName = "Name1",
            LastName = "LastName1",
        };
        
        var builder = new MapperConfigurationBuilder();
        builder.CreateMap<Person, PersonDto>()
            .ForMember(x => x.FullName, opt =>
                opt.Map(source => source.FirstName + " " + source.LastName));
        // .ForMember(x => x.Address, opt => opt.Ignore());

        var mapper = new ExpressionBasedMapper(builder.Build());
        var dto = mapper.Map<PersonDto>(person1);

        Assert.Equal(dto.LastName, person1.LastName);
        Assert.Equal(dto.FirstName, person1.FirstName);
    }
}