namespace PngSharp.Api;

public readonly struct AncillaryChunk<T>
{
    private readonly bool m_HasValue;
    private readonly T? m_Value;

    public AncillaryChunk()
    {
        m_HasValue = false;
    }
    
    private AncillaryChunk(T value)
    {
        m_Value = value;
        m_HasValue = true;
    }
    
    public bool TryGetData(out T value)
    {
        value = m_Value;
        return value != null;
    }

    public static AncillaryChunk<T> Of(T value)
    {
        return new AncillaryChunk<T>(value);
    }
}