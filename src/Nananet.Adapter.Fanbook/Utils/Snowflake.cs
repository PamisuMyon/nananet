using System.Numerics;

namespace Nananet.Adapter.Fanbook.Utils;

public class Snowflake
{
    private static BigInteger EPOCH = 0;
    private static BigInteger? SHARD_ID = null;

    public static string Generate(BigInteger timestamp, BigInteger? shardId = null, BigInteger? sequence = null)
    {
        timestamp = timestamp != 0 ? timestamp : DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        shardId = shardId ?? SHARD_ID ?? new Random().Next(0, 1024);
        sequence = sequence ?? new Random().Next(0, 4096);

        var snowflake = ((timestamp - EPOCH) << 22) | (shardId.Value % 1024) << 12 | sequence.Value;
        return snowflake.ToString();
    }

}