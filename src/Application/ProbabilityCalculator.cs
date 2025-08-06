using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Application.Abstractions;

namespace Application;
public sealed class ProbabilityCalculator : IProbabilityCalculator
{
    private readonly IRandomDoubleProvider _randomProvider;

    public ProbabilityCalculator(IRandomDoubleProvider randomProvider)
    {
        _randomProvider = randomProvider ?? throw new ArgumentNullException(nameof(randomProvider));
    }

    public double[] CalculateWinnerProbabilities(int numberOfRunners, double bookmakerMargin)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(numberOfRunners, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(bookmakerMargin);

        double[] winnerProbabilities = new double[numberOfRunners];

        // Generate secure random doubles between 0 and 1
        for (int i = 0; i < numberOfRunners; i++)
        {
            winnerProbabilities[i] = _randomProvider.NextDouble();
        }

        // Normalize so sum = 1
        double sum = winnerProbabilities.Sum();
        for (int i = 0; i < numberOfRunners; i++)
        {
            winnerProbabilities[i] /= sum;
        }

        // Apply bookmaker margin: scale to sum > 1
        double scale = (1 + bookmakerMargin) / winnerProbabilities.Sum();
        for (int i = 0; i < numberOfRunners; i++)
        {
            winnerProbabilities[i] *= scale;
        }

        return winnerProbabilities;
    }
}
