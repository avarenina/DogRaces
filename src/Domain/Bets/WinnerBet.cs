namespace Domain.Bets;

public class WinnerBet : Bet
{
    public override BetStatus GetStatusOnRaceEnd(List<int> raceResult)
    {
        return Runners[0] == raceResult[0]
                ? BetStatus.Winning
                : BetStatus.Losing;
    }
}
