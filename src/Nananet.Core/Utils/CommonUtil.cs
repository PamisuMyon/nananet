using System.Dynamic;

namespace Nananet.Core.Utils;

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
    
    public static T MergeWith<T>(this T left, T right) where T : class
    {
        foreach (var p in typeof(T).GetProperties())
        {
            var rightValue = p.GetValue(right);
            if (rightValue == default) continue;
            p.SetValue(left, rightValue);
        }
        return left;
    }

    public static T? GetElemSafe<T>(this IList<T> list, int index, T? defaultValue = default)
    {
        if (index < 0 || index > list.Count - 1) return defaultValue;
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
    
    public static List<List<T>> Combinations<T>(List<T> items, int limit) {
        return DoCombination(items, limit, 0);
    }
    
    private static List<List<T>> DoCombination<T>(List<T> items, int limit, int index) {
        // 递归跳出条件，组合数量为1时，将每个元素作为单独的数组返回，和上一层组合
        if (limit == 1 || index == items.Count - 1) {
            List<List<T>> list = new();
            var elemLists = items.Take(new Range(index, Index.End))
                .Select(elem => new List<T> { elem });
            list.AddRange(elemLists);
            return list;
        }
        List<List<T>> results = new();
        // 从第0个元素依次向后，将自身与下一层数组组合
        for (; index <= items.Count - limit; index++) {
            var list = DoCombination(items, limit - 1, index + 1);
            list.ForEach(elem =>
            {
                elem.Insert(0, items[index]);
            });
            results.AddRange(list);
        }
        return results;
    }
    
    public static void Shuffle<T>(this List<T> list)
    {
        var rnd = new Random();
        var n = list.Count;
        while (n > 1)
        {
            n--;
            var k = rnd.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }

    public static bool NullOrEmpty(this string? it) => string.IsNullOrEmpty(it);

    public static bool NotNullNorEmpty(this string? it) => !string.IsNullOrEmpty(it);
    
}