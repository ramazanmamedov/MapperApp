using System.Reflection;

namespace MapperApp;

public class Mapper : IMapper
{
    private readonly MapperConfiguration _configuration;
    private readonly IEnumerable<ICustomTypeMapper> _customTypeMappers;

    public Mapper(MapperConfiguration configuration, IEnumerable<ICustomTypeMapper> customTypeMappers)
    {
        _configuration = configuration;
        _customTypeMappers = customTypeMappers.ToList() ;
    }
    public TDest Map<TDest, TSource>(TSource? source) where TDest : new()
        => Map<TDest>(source);

    public TDest Map<TDest>(object? source)
        => (TDest) Map(typeof(TDest), source);

    public object Map(Type destType, object? source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        
        var sourceProps = source.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
        var destProps = destType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

        if (TryCustomMap(destType, source, out var customResult)) 
            return customResult;

        var result = Activator.CreateInstance(destType);
        var config = _configuration.TypeMappings.FirstOrDefault(x => x.SourceType == source.GetType() && x.DestType == destType);
        MapProperties(source, destProps, config, sourceProps, result);

        return result!;
    }

    private void MapProperties(object source, PropertyInfo[] destProps, TypeMappingConfiguration? config,
        PropertyInfo[] sourceProps, object? result)
    {
        foreach (var destProp in destProps)
        {
            bool hasConfig = false;
            object? value = null;
            var memberMapping = config?.MemberMappings.FirstOrDefault(x => x.MemberName == destProp.Name);
            if (memberMapping != null)
            {
                hasConfig = true;
                if (memberMapping.Action is IgnoreMappingAction)
                    continue;
                else if (memberMapping.Action is MapAction mapAction)
                {
                    value = mapAction.Action.DynamicInvoke(source);
                }
            }

            if (!hasConfig)
            {
                var sourceProp = sourceProps.FirstOrDefault(p => p.Name == destProp.Name);
                if (sourceProp != null && sourceProp.CanRead && destProp.CanWrite)
                {
                    value = sourceProp.GetValue(source);
                }
            }

            if (value != null && !value.GetType().IsAssignableTo(destProp.PropertyType))
                value = Map(destProp.PropertyType, value);

            destProp.SetValue(result, value);
        }
    }

    private bool TryCustomMap(Type destType, object source, out object o)
    {
        var customMapper = _customTypeMappers.FirstOrDefault(x => x.CanMap(source.GetType(), destType));
        if (customMapper != null)
        {
            o = customMapper.Map(this, destType, source);
            return true;
        }

        o = null;
        return false;
    }
}

public interface ICustomTypeMapper
{
    bool CanMap(Type source, Type dest);
    object Map(Mapper mapper, Type dest, object source);
}

public class ListToArrayMapper : ICustomTypeMapper
{
    public bool CanMap(Type source, Type dest)
    {
        return dest.IsSZArray && (source.IsGenericType && source.GetGenericTypeDefinition() == typeof(List<>));
    }

    public object Map(Mapper mapper, Type dest, object source)
    {
        var method = GetType().GetMethod(nameof(MapInternal), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(source.GetType().GetGenericArguments()[0], dest.GetElementType()!);

        return method.Invoke(null, new object[] {mapper, source})!;
    }

    private static TDest[] MapInternal<TSource, TDest>(IMapper mapper, List<TSource> sources) =>
        sources.Select(x => mapper.Map<TDest>(x)).ToArray();
}