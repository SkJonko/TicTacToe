namespace TicToe.Infinite.Models;

public record class WinnerInfo
{
    /// <summary>
    /// Refer's to the player who won (e.g., "X" or "O").
    /// </summary>
    public string Player { get; private set; } = string.Empty;

    /// <summary>
    /// The total count of wins for the player.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// Average moves taken by the player.
    /// </summary>
    public int AverageMoves { get; private set; }

    /// <summary>
    /// The total moves.
    /// </summary>
    public int TotalMoves { get; private set; }

    /// <summary>
    /// Initializes a new instance of the WinnerInfo class with the specified player name, win count, and total moves.
    /// </summary>
    /// <remarks>The average number of moves per win is calculated automatically based on the provided count
    /// and totalMoves values.</remarks>
    /// <param name="player">The name of the player associated with this winner information. Cannot be null.</param>
    /// <param name="count">The number of wins recorded for the player. Must be zero or greater.</param>
    /// <param name="totalMoves">The total number of moves made by the player across all recorded wins. Must be zero or greater. Defaults to 0 if
    /// not specified.</param>
    public WinnerInfo(string player, int count, int totalMoves = 0)
    {
        Player = player;
        Count = count;
        TotalMoves = totalMoves;
        AverageMoves = count > 0 ? totalMoves / count : 0;
    }
}