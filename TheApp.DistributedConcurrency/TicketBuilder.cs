using System.Security.Cryptography;
using System.Text;

namespace TheApp.DistributedConcurrency;

public static class TicketBuilder
{
    public static string Build(string kind, params Type[] types)
    {
        var data = types.Aggregate(
            new StringBuilder(kind),
            (aggr, type) => aggr.Append(type.FullName),
            aggr => aggr.ToString());

        return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(data)));
    }
}
