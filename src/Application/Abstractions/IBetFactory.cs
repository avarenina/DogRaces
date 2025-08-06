using Domain.Bets;
using Domain.Races;

namespace Application.Abstractions;
public interface IBetFactory
{
    List<Bet> CreateWinnerBets(Race race);
    List<Bet> CreateWithinFirstThreeBets(Race race);
}
