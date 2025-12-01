namespace TicToe.Infinite.Services;

public class TicToeGameService
{
    #region Properties

    public char[,] Board { get; private set; } = new char[3, 3];

    public char CurrentPlayer { get; private set; } = 'X';

    public bool GameOver { get; private set; } = false;

    public bool Won { get; private set; } = false;

    public int MovesMade { get; private set; } = 0;

    private Queue<(int r, int c)> moveOrder = new();

    public bool UseAI = false;

    public char AIPlayer = 'O';

    #endregion

    public event Action<(int r, int c)>? CellRemoved;

    public event Action<(int r, int c)>? CellWillBeRemoved;

    /// <summary>
    /// Make move at (row, col) for the game.
    /// </summary>
    /// <param name="row">The row</param>
    /// <param name="col">The column</param>
    /// <returns>A <see cref="bool"/> to indicates if the moves was successful completed.</returns>
    public bool MakeMove(int row, int col)
    {
		if (GameOver)
		{
			return false;
		}

		if (Board[row, col] != '\0')
		{
			return false;
		}

		// Remove oldest if exceeding capacity
		if (moveOrder.Count >= 6)
        {
            var oldest = moveOrder.Dequeue();
            Board[oldest.r, oldest.c] = '\0';
            CellRemoved?.Invoke(oldest);
        }

        // Place symbol
        Board[row, col] = CurrentPlayer;
        moveOrder.Enqueue((row, col));
        MovesMade++;

        // Check win
        Won = CheckWin(CurrentPlayer);

        if (Won)
        {
            GameOver = true;
            return true;
        }

        // Trigger breathing on oldest cell before removal
        if (moveOrder.Count >= 6)
        {
            var oldest = moveOrder.Peek();
            CellWillBeRemoved?.Invoke(oldest);
        }

        // Swap players
        CurrentPlayer = CurrentPlayer == 'X' ? 'O' : 'X';

        return true;
    }

    /// <summary>
    /// Reset the game to initial state.
    /// </summary>
    public void Reset()
    {
        Board = new char[3, 3];
        moveOrder.Clear();
        CurrentPlayer = 'X';
        GameOver = false;
        MovesMade = 0;
        Won = false;
    }

    /// <summary>
    /// Returns AI move if available.
    /// </summary>
    /// <returns></returns>
    public (int r, int c)? GetAIMove()
    {
        var free = new List<(int r, int c)>();

        for (int r = 0; r < 3; r++)
		{
			for (int c = 0; c < 3; c++)
			{
				if (Board[r, c] == '\0')
				{
					free.Add((r, c));
				}
			}
		}

		if (free.Count == 0)
		{
			return null;
		}

		var rand = new Random();

        return free[rand.Next(free.Count)];
    }

    private bool CheckWin(char p)
    {
        for (int r = 0; r < 3; r++)
		{
			if (Board[r, 0] == p && Board[r, 1] == p && Board[r, 2] == p)
			{
				return true;
			}
		}

		for (int c = 0; c < 3; c++)
		{
			if (Board[0, c] == p && Board[1, c] == p && Board[2, c] == p)
			{
				return true;
			}
		}

		if (Board[0, 0] == p && Board[1, 1] == p && Board[2, 2] == p)
		{
			return true;
		}

		return Board[0, 2] == p && Board[1, 1] == p && Board[2, 0] == p;
	}
}