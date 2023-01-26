using System.Text.RegularExpressions;

namespace Nananet.App.Nana.Commons;

public static class NaUtil
{
    public static string RemoveLabel(string text) {
        var reg = new Regex("<[\\@\\$\\/].*?>", RegexOptions.Multiline);
        return reg.Replace(text, "");
    }
    
    public static string GetRarityText(int? rarity) {
        if (rarity == null) return "";
        return rarity switch
        {
            0 => "⬜",
            1 => "🟩",
            2 => "🟦",
            3 => "🟪",
            4 => "🟨",
            5 => "🟧",
            _ => ""
        };
    }
    
}