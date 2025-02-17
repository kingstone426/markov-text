namespace MarkovText;

/// <summary>
/// Thanks to Jon Skeet for the idea/implementation for this class.
/// https://stackoverflow.com/a/7244729
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class ArrayEqualityComparer<T> : IEqualityComparer<T[]>
{
    // You could make this a per-instance field with a constructor parameter
    private static readonly EqualityComparer<T> ElementComparer
        = EqualityComparer<T>.Default;

    public bool Equals(T[]? first, T[]? second)
    {
        if (first == second)
        {
            return true;
        }
        if (first == null || second == null)
        {
            return false;
        }
        if (first.Length != second.Length)
        {
            return false;
        }
        for (var i = 0; i < first.Length; i++)
        {
            if (!ElementComparer.Equals(first[i], second[i]))
            {
                return false;
            }
        }
        return true;
    }

    public int GetHashCode(T[]? array)
    {
        unchecked
        {
            if (array == null)
            {
                return 0;
            }
            var hash = 17;
            foreach (var element in array)
            {
                hash = hash * 31 + ElementComparer.GetHashCode(element!);
            }
            return hash;
        }
    }
}

public sealed class StringArrayEqualityComparer : IEqualityComparer<string[]>
{
    public bool Equals(string[]? first, string[]? second)
    {
        if (first == second)
        {
            return true;
        }
        if (first == null || second == null)
        {
            return false;
        }
        if (first.Length != second.Length)
        {
            return false;
        }
        for (var i = 0; i < first.Length; i++)
        {
            if (!first[i].Equals(second[i]))
            {
                return false;
            }
        }
        return true;
    }

    public int GetHashCode(string[] array)
    {
        unchecked
        {
            var hash = 17;
            foreach (var element in array)
            {
                hash = hash * 31 + element.GetStableHashCode();
            }
            return hash;
        }
    }
}