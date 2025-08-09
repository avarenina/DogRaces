namespace Domain.Abstractions;
public interface IProbabilityCalculator
{
    double[] CalculateWinnerProbabilities(int numberOfRunners, double bookmakerMargin);
}
