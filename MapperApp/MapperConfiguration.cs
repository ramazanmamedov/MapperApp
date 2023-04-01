using System.Linq.Expressions;

namespace MapperApp;

public record MapperConfiguration(IEnumerable<TypeMappingConfiguration> TypeMappings);
public record TypeMappingConfiguration(Type SourceType, Type DestType, IEnumerable<MemberMappingConfiguration> MemberMappings  );
public record MemberMappingConfiguration(string MemberName, IMappingAction Action);
public interface IMappingAction{}

public record IgnoreMappingAction : IMappingAction{}
public record MapAction(Delegate Action) : IMappingAction{}

public interface IMemberMappingConfigurationBuilder
{
    MemberMappingConfiguration Build();
}

public class MemberMappingConfigurationBuilder<TSource, TMember> : IMemberMappingConfigurationBuilder
{
    private readonly string _memberName;
    private bool _ignore;
    private Func<TSource, TMember> _mapAction;

    public MemberMappingConfigurationBuilder(string memberName)
    {
        _memberName = memberName;
    }
    public void Ignore()
    {
        _ignore = true;
    }

    public void Map(Func<TSource, TMember> mapAction)
    {
        _mapAction = mapAction;
    }

    public MemberMappingConfiguration Build()
    {
        return new MemberMappingConfiguration(_memberName, _ignore ? new IgnoreMappingAction() : new MapAction(_mapAction)); 
    }
}

public interface ITypeMappingBuilder
{
    TypeMappingConfiguration Build();
}

public class TypeMappingBuilder<TSource, TDest> : ITypeMappingBuilder
{
    private readonly List<IMemberMappingConfigurationBuilder> _builders = new();
    
    public TypeMappingBuilder<TSource, TDest>  ForMember<TProp>(string destMemberName, Action<MemberMappingConfigurationBuilder<TSource, TProp>> memberOptions)
    {
        var builder = new MemberMappingConfigurationBuilder<TSource, TProp>(destMemberName);
        _builders.Add(builder);
        memberOptions(builder);
        return this;
    }

    public TypeMappingBuilder<TSource, TDest> ForMember<TProp>(Expression<Func<TDest, TProp>> destMember, Action<MemberMappingConfigurationBuilder<TSource, TProp>> memberOptions)
    {
        var memberName = ((MemberExpression) destMember.Body).Member.Name;
        return ForMember(memberName, memberOptions);
    }

    public TypeMappingConfiguration Build()
    {
        return new TypeMappingConfiguration(typeof(TSource), typeof(TDest), _builders.Select(x => x.Build()));
    }
}

public class MapperConfigurationBuilder
{
    private readonly List<ITypeMappingBuilder> _builders = new();

    public TypeMappingBuilder<TSource, TDest> CreateMap<TSource, TDest>()
    {
        var builder =  new TypeMappingBuilder<TSource, TDest>();
        _builders.Add(builder);
        return builder;
    }

    public MapperConfiguration Build()
    {
       return new MapperConfiguration(_builders.Select(x => x.Build()));
    }

}