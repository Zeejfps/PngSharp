namespace PngSharp.Api;

public readonly struct AncillaryChunk<T>
{
    private readonly bool m_HasValue;
    private readonly T? m_Value;
    
    
    private AncillaryChunk(T value, bool hasValue)
    {
        m_Value = value;
        m_HasValue = hasValue;
    }
    
    public bool TryGetData(out T value)
    {
        value = m_Value;
        return m_HasValue;
    }

    public static AncillaryChunk<T> Of(T value)
    {
        return new AncillaryChunk<T>(value, true);
    }
}