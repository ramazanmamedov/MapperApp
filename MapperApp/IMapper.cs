namespace MapperApp;

public interface IMapper
{
    TDest Map<TDest, TSource>(TSource source) where TDest : new();
    TDest Map<TDest>(object source);
    object Map(Type destType, object source);
}

