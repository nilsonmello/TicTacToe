public interface IGameResult
{
    Player GetWinner();
    WinState GetWinCondition();
}

class GameResult : IGameResult
{
    private Player winner;
    private WinState winCondition;

    public GameResult(Player winner, WinState winCondition)
    {
        this.winner = winner;
        this.winCondition = winCondition;
    }

    public Player GetWinner()
    {
        return winner;
    }

    public WinState GetWinCondition()
    {
        return winCondition;
    }
}