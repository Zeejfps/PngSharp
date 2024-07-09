namespace PngSharp.Api;

public readonly struct AncillaryChunk<T>
{
    private readonly T m_Value;

    private AncillaryChunk(T value)
    {
        m_Value = value;
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