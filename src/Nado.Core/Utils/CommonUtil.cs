using System.Dynamic;
using System.Text.RegularExpressions;

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
    
}