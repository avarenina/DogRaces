namespace Domain.Bets;

public class WithinFirstThreeBet : Bet
{
    public override BetStatus GetStatusOnRaceEnd(List<int> raceResult)
    {
        return Runners[0] == raceResult[0] || Runners[0] == raceResult[1] || Runners[0] == raceResult[2]
                ? BetStatus.Winning
                : BetStatus.Losing;
    }
}
