using System;
using System.Collections.Generic;

public static class GlobalExtensions
{
    public static void Remove<T>(this IList<T> list, Predicate<T> predicate)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (predicate(list[i]))
                list.RemoveAt(i);
        }
    }
}
