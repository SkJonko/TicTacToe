namespace TicToe.Infinite.Data.Models;

/// <summary>
/// Refers to a Tic Tac Toe Game.
/// </summary>
public class Game
{
    /// <summary>
    /// The Identifier of the Game.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The winner of the Game.
    /// </summary>
    public string Winner { get; set; } = string.Empty;

    /// <summary>
    /// The moves until someone wins.
    /// </summary>
    public int Moves { get; set; } = 0;
}
