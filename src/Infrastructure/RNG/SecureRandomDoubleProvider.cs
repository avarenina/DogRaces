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
        return ulongRand / (double)ulong.MaxValue;
    }
}
