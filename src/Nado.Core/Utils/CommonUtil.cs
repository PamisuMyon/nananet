using System.Dynamic;

namespace Nado.Core.Utils;

public static class CommonUtil
{
    public static ExpandoObject MergeDynamic<TLeft, TRight>(this TLeft left, TRight right)
    {
        var expando = new ExpandoObject();
        IDictionary<string, object?> dict = expando;
        foreach (var p in typeof(TLeft).GetProperties())
            dict[p.Name] = p.GetValue(left);
        foreach (var p in typeof(TRight).GetProperties())
            dict[p.Name] = p.GetValue(right);
        return expando;
    }
    
    public static T MergeWith<T>(this T left, T right) {
        foreach (var p in typeof(T).GetProperties())
        {
            p.SetValue(left, p.GetValue(right));
        }
        return left;
    }

    public static T? GetElemSafe<T>(this IList<T> list, int index)
    {
        if (index < 0 || index > list.Count - 1) return default;
        return list[index];
    }

    public static int RandomInt(int minInclusive, int maxInclusive)
    {
         return new Random().Next(minInclusive, maxInclusive + 1);
    }

    public static float RandomSingle(float minInclusive, float maxExclusive)
    {
        return new Random().NextSingle() * (maxExclusive - minInclusive) + minInclusive;
    }

    public static T RandomElem<T>(this IList<T> list)
    {
        return list[RandomInt(0, list.Count - 1)];
    }
    
    public static T RandomElem<T>(this T[] arr)
    {
        return arr[RandomInt(0, arr.Length - 1)];
    }

    public static int GetInt(string? str, int defaultValue)
    {
        if (str == null) return defaultValue;
        try
        {
            var i = int.Parse(str);
            return i;
        }
        catch (Exception e)
        {
            return defaultValue;
        }
    }
    
}