using System.Security.Cryptography;
using Application.Abstractions;

namespace Infrastructure.RNG;
public class SecureRandomDoubleProvider : IRandomDoubleProvider
{
    public double NextDouble()
    {
        Span<byte> bytes = stackalloc byte[8];
        RandomNumberGenerator.Fill(bytes);
        ulong ulongRand = BitConverter.ToUInt64(bytes);

        // Use only the top 53 bits for double precision
        ulongRand >>= 11;
        const double divisor = 1.0 / (1UL << 53); // 1 / 2^53

        return ulongRand * divisor;
    }
}
