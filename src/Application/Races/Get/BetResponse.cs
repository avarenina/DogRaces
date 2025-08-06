using Domain.Bets;
using Domain.Races;

namespace Application.Races.Get;
public class BetResponse
{
    public Guid Id { get; set; }
    public decimal Odds { get; set; }
    public List<int> Runners { get; set; }
    public BetStatus Status { get; set; }
    public BetType Type { get; set; }
}
