using Domain.Bets;
using Domain.Races;

namespace Application.Abstractions;
public interface IBetFactory
{
    List<Bet> Create(Race race);
}
