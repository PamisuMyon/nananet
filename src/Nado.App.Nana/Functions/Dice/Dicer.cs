using Nado.Core.Utils;

namespace Nado.App.Nana.Functions.Dice;

public struct DiceOptions
{
    public int Add;
    public int Rounds;
    public int Times;
    public int Dice;
    public int Add2;
}

public static class Dicer
{
    public static string Roll(DiceOptions o)
    {
        var result = "";
        if (o.Rounds <= 0 || o.Times <= 0 || o.Dice <= 0)
        {
            return result;
        }
        
        for (var i = 0; i < o.Rounds; i++)
        {
            var num = 0;
            var sum = 0;
            if (o.Rounds > 1)
            {
                result += $"{i + 1} »  ";
            }
            if (o.Add != 0)
            {
                result += $"{o.Add}+";
            }
            result += $"{o.Times}d{o.Dice}";
            if (o.Add2 != 0)
            {
                result += $"+{o.Add2}";
            }
            result += " = ";
            if (o.Times == 1)
            {
                num = CommonUtil.RandomInt(1, o.Dice);
                if (o.Add != 0 || o.Add2 != 0)
                {
                    if (o.Add != 0)
                    {
                        result += $"{o.Add}+";
                    }
                    result += num;
                    if (o.Add2 != 0)
                    {
                        result += $"+{o.Add2}";
                    }
                    sum = o.Add + num + o.Add2;
                    result += $" = {sum}";
                }
                else
                {
                    result += o.Add + num + o.Add2;
                }
            }
            else
            {
                if (o.Add != 0)
                {
                    result += $"{o.Add}+";
                }
                sum += o.Add;
                for (var j = 0; j < o.Times; j++)
                {
                    num = CommonUtil.RandomInt(1, o.Dice);
                    sum += num;
                    result += num;
                    if (j < o.Times - 1)
                    {
                        result += '+';
                    }
                }
                if (o.Add2 != 0)
                {
                    result += $"+{o.Add2}";
                }
                sum += o.Add2;
                result += $" = {sum}";
            }
            if (o.Rounds != 1 && i < o.Rounds - 1)
            {
                result += '\n';
            }
        }
        return result;
    }

    public static string Rolls(IEnumerable<DiceOptions> options,
        string[] errorHints,
        string[] tooManyHints,
        int maxRounds = 20,
        int maxTimes = 50)
    {
        var results = new List<string>();
        foreach (var o in options)
        {
            var result = "";
            if (o.Rounds <= 0 || o.Times <= 0 || o.Dice <= 0)
            {
                result = errorHints.RandomElem();
            }
            else if (o.Rounds >= maxRounds || o.Times > maxTimes)
            {
                result = tooManyHints.RandomElem();
            }
            else
            {
                result = Roll(o);
            }
            results.Add(result);
        }
        var reply = "";
        if (results.Count > 1)
        {
            for (var i = 0; i < results.Count; i++)
            {
                var splits = results[i].Split('\n');
                for (var j = 0; j < splits.Length; j++)
                {
                    reply += $"{i + 1} ·  ";
                    reply += splits[j];
                    if (j < splits.Length - 1)
                    {
                        reply += '\n';
                    }
                }
                if (i < results.Count - 1)
                {
                    reply += '\n';
                }
            }
        }
        else if (results.Count == 1)
        {
            reply = results[0];
        }
        else
        {
            reply = errorHints.RandomElem();
        }
        return reply;
    }
    
}