using Domain.Bets;
using Domain.Races;

namespace Domain.Abstractions;
public interface IBetFactory
{
    List<Bet> Create(Race race);
}
