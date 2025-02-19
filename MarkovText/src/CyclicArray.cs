namespace MarkovText;

public class CyclicArray<T>
{
    private readonly T[] data;

    public CyclicArray(int size)
    {
        if (size <= 0)
        {
            throw new ArgumentException();
        }

        data = new T[size];
    }

    public T this[int index]
    {
        get => data[Index(index)];
        set => data[Index(index)] = value;
    }

    private int Index(int index)
    {
        while (index < 0)
        {
            index += data.Length;   // This could simply be calculated
        }

        return index % data.Length;
    }

    public T[] CreateOffsetArray(int offset)
    {
        var ret = new T[data.Length];

        for (var i = 0; i < data.Length; i++)
        {
            ret[i] = data[(i + offset) % data.Length];
        }

        return ret;
    }
}
