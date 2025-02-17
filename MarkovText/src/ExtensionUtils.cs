namespace MarkovText;

public static class ExtensionUtils
{
    public static void Add<T1, T2>(this Dictionary<T1, List<T2>> map, T1 key, T2 value) where T1 : notnull
    {
        if (!map.TryGetValue(key, out var list))
        {
            list = new List<T2>();
            map[key] = list;
        }
        list.Add(value);
    }

    public static T Random<T>(this List<T> list, IRandomNumberGenerator rnd)
    {
        return list[rnd.Next(list.Count)];
    }

    /// <summary>
    /// Thanks to Scott Chamberlain for the idea/implementation for this method.
    /// https://stackoverflow.com/a/36845864
    /// </summary>
    public static int GetStableHashCode(this string str)
    {
        unchecked
        {
            var hash1 = 5381;
            var hash2 = hash1;

            for(var i = 0; i < str.Length && str[i] != '\0'; i += 2)
            {
                hash1 = ((hash1 << 5) + hash1) ^ str[i];
                if (i == str.Length - 1 || str[i+1] == '\0')
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ str[i+1];
            }

            return hash1 + (hash2*1566083941);
        }
    }
}