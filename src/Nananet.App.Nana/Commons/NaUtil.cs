using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Nananet.App.Nana.Commons;

public static class NaUtil
{
    public static string RemoveLabel(string text)
    {
        var reg = new Regex("<[\\@\\$\\/].*?>", RegexOptions.Multiline);
        return reg.Replace(text, "");
    }

    public static string GetRarityText(int? rarity)
    {
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
    
    public static bool IsValidMonthDate(int month, int day) {
        if (month > 12 || month < 1) return false;
        if (day < 1 || day > 31) return false;
        if ((month == 4 || month == 6 || month == 9 || month == 11) && (day > 30)) return false;
        if (month == 2 && day > 29) return false;
        return true;
    }
    
}

public static class ZhDigitToArabic
{
    private static readonly List<char> zh = new() { '零', '一', '二', '三', '四', '五', '六', '七', '八', '九' };
    private static readonly List<string> unit = new()  { "千", "百", "十" };
    private static readonly List<string> quot = new() { "万", "亿", "兆", "京", "垓", "秭", "穰", "沟", "涧", "正", "载", "极", "恒河沙", "阿僧祗", "那由他", "不可思议", "无量", "大数" };

    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    public static double Convert(string digit)
    {
        double result = 0;
        double quotFlag = -1;
        
        if (digit[0] == '十')
        {
            if (digit.Length == 1)
                return 10;
            if (digit.Length == 2)
                return 10 + GetNumber(digit[1]);
        }

        for (var i = digit.Length - 1; i >= 0; i--)
        {
            if (zh.IndexOf(digit[i]) > -1)
            {
                // 数字
                if (quotFlag != -1)
                {
                    result += quotFlag * GetNumber(digit[i]);
                }
                else
                {
                    result += GetNumber(digit[i]);
                }
            }
            else if (unit.IndexOf(digit[i].ToString()) > -1)
            {
                // 十分位
                if (quotFlag != -1)
                {
                    result += quotFlag * GetUnit(digit[i].ToString()) * GetNumber(digit[i - 1]);
                }
                else
                {
                    result += GetUnit(digit[i].ToString()) * GetNumber(digit[i - 1]);
                }

                --i;
            }
            else if (quot.IndexOf(digit[i].ToString()) > -1)
            {
                // 万分位
                if (unit.IndexOf(digit[i - 1].ToString()) > -1)
                {
                    if (GetNumber(digit[i - 1]) != 0)
                    {
                        result += GetQuot(digit[i].ToString()) * GetNumber(digit[i - 1]);
                    }
                    else
                    {
                        result += GetQuot(digit[i].ToString()) * GetUnit(digit[i - 1].ToString()) * GetNumber(digit[i - 2]);
                        quotFlag = GetQuot(digit[i].ToString());
                        --i;
                    }
                }
                else
                {
                    result += GetQuot(digit[i].ToString()) * GetNumber(digit[i - 1]);
                    quotFlag = GetQuot(digit[i].ToString());
                }

                --i;
            }
        }

        return result;
    }

    // 返回中文大写数字对应的阿拉伯数字
    private static double GetNumber(char num)
    {
        for (var i = 0; i < zh.Count; i++)
        {
            if (zh[i] == num)
            {
                return i;
            }
        }
        return 0;
    }

    // 取单位
    private static double GetUnit(string num)
    {
        for (var i = unit.Count; i > 0; i--)
        {
            if (num == unit[i - 1])
            {
                return Math.Pow(10, 4 - i);
            }
        }
        return 1;
    }

    // 取分段
    private static double GetQuot(string q)
    {
        for (var i = 0; i < quot.Count; i++)
        {
            if (q == quot[i])
                return Math.Pow(10, (i + 1) * 4);
        }
        return 1;
    }
}