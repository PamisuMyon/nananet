namespace Nado.Core.Utils;

public class SpamInfo
{
    public string Id { get; set; }
    public int CommandCount { get; set; }
    public long CommandTime { get; set; }
    public long CooldownTime { get; set; }
    public int FailedTimes { get; set; }
}

public struct CheckResult
{
    public bool Pass { get; set; }
    public long Remain { get; set; }
    public int FailedTimes { get; set; }
}

public class Spam
{
    protected int _interval;
    protected int _threshold;
    protected long _cooldown;
    protected Dictionary<string, SpamInfo> _infos;

    public Spam(int interval = 45000, int threshold = 3, long cooldown = 60000)
    {
        _infos = new Dictionary<string, SpamInfo>();
        Init(interval, threshold, cooldown);
    }

    public void Init(int interval = 45000, int threshold = 3, long cooldown = 60000)
    {
        _interval = interval;
        _threshold = threshold;
        _cooldown = cooldown;
    }

    public CheckResult Check(string id)
    {
        if (!_infos.ContainsKey(id))
            return new CheckResult
            {
                Pass = true,
            };

        var info = _infos[id];
        var time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        if (info.CooldownTime >= time)
        {
            info.FailedTimes++;
            return new CheckResult
            {
                Pass = false,
                Remain = info.CooldownTime - time,
                FailedTimes = info.FailedTimes
            };
        }

        return new CheckResult
        {
            Pass = true,
        };
    }

    public void Record(string id)
    {
        var time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        if (!_infos.ContainsKey(id))
        {
            var info = new SpamInfo
            {
                Id = id,
                CommandCount = 1,
                CommandTime = time,
                CooldownTime = -1,
                FailedTimes = 0
            };
            _infos.Add(id, info);
            if (_threshold <= 1)
            {
                info.CooldownTime = time + _cooldown;
                info.CommandCount = 0;
                info.FailedTimes = 0;
            }
        }
        else
        {
            var info = _infos[id];
            if (info.CommandTime >= time - _interval || _threshold <= 1)
            {
                info.CommandCount++;
                if (info.CommandCount >= _threshold)
                {
                    info.CooldownTime = time + _cooldown;
                    info.CommandCount = 0;
                    info.FailedTimes = 0;
                }
            }
            else
            {
                info.CommandCount = 1;
            }
            info.CommandTime = time;
        }
    }

    public void Reset(string id)
    {
        if (_infos.ContainsKey(id))
        {
            _infos[id].CommandCount = 0;
            _infos[id].CommandTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            _infos[id].CooldownTime = -1;
            _infos[id].FailedTimes = 0;
        }
    }
    
}