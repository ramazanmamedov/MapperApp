using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace MapperApp;

public class ExpressionBasedMapper : IMapper
{
    private record struct TypePair(Type Source, Type Dest);
    private readonly MapperConfiguration _configuration;
    private readonly ConcurrentDictionary<TypePair, Func<object, object>> _mapFunctions = new();

    public ExpressionBasedMapper(MapperConfiguration configuration)
    {
        _configuration = configuration;
    }


    public TDest Map<TDest, TSource>(TSource? source) where TDest : new()
        => Map<TDest>(source);

    public TDest Map<TDest>(object? source)
        => (TDest) Map(typeof(TDest), source!);

    public object Map(Type destType, object source)
    {
        return _mapFunctions.GetOrAdd(new(source.GetType(), destType), BuildExecutionPlan)(source);
    }

    private Func<object, object> BuildExecutionPlan(TypePair arg)
    {
        var source = Expression.Parameter(arg.Source, "source");
        var sourceProps = arg.Source.GetProperties();
        var result = Expression.Variable(arg.Dest, "result");
        var body = new List<Expression> {Expression.Assign(result, Expression.New(arg.Dest))};
        foreach (var destProp in arg.Dest.GetProperties())
        {
            var sourceProp = sourceProps.FirstOrDefault(x => x.Name == destProp.Name);
            if (sourceProp == null)
                continue;
            
            body.Add(Expression.Assign(Expression.PropertyOrField(result, sourceProp!.Name), Expression.PropertyOrField(source, destProp.Name)));
        }
        body.Add(result);
        var block = Expression.Block(new [] {result}, body);
        return (Func<object, object>) GetType()
            .GetMethod(nameof(BuildLambda), BindingFlags.NonPublic | BindingFlags.Instance)!
            .MakeGenericMethod(arg.Source, arg.Dest)
            .Invoke(this, new object[] {block, source})!;
    }

    private Func<object, object> BuildLambda<TSource, TDest>(Expression block, ParameterExpression parameter)
    {
        var result = Expression.Lambda<Func<TSource, TDest>>(block, parameter).Compile();

        return s => result((TSource) s)!;
    }
}