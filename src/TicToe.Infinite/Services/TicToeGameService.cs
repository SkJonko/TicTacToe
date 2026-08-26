using System;
using System.Collections.Generic;
using System.Linq;

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

	private const int MaxSearchDepth = 10;

	private static readonly Random _random = new();

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
	/// Returns the best AI move using depth-limited minimax with alpha-beta pruning.
	/// Wins immediately when possible, blocks the opponent's immediate win, and otherwise
	/// looks several moves ahead (accounting for the "oldest piece vanishes" rule) to pick
	/// the strongest available move. Ties are broken randomly among equally good moves.
	/// </summary>
	public (int r, int c)? GetAIMove()
	{
		if (GameOver)
		{
			return null;
		}

		var free = GetFreeCells(Board);

		if (free.Count == 0)
		{
			return null;
		}

		char aiPlayer = AIPlayer;
		char humanPlayer = aiPlayer == 'X' ? 'O' : 'X';

		// Work on copies so the search never mutates the real game state
		var board = CloneBoard(Board);
		var order = moveOrder.Select(m => (m.r, m.c, symbol: Board[m.r, m.c])).ToList();

		int bestScore = int.MinValue;
		var bestMoves = new List<(int r, int c)>();

		int alpha = int.MinValue;
		int beta = int.MaxValue;

		foreach (var move in free)
		{
			var removed = ApplyMove(order, board, move.r, move.c, aiPlayer);
			int score = Minimax(board, order, 1, false, alpha, beta, aiPlayer, humanPlayer);
			UndoMove(order, board, move.r, move.c, removed);

			if (score > bestScore)
			{
				bestScore = score;
				bestMoves.Clear();
				bestMoves.Add(move);
			}
			else if (score == bestScore)
			{
				bestMoves.Add(move);
			}

			alpha = Math.Max(alpha, bestScore);
		}

		return bestMoves[_random.Next(bestMoves.Count)];
	}

	private static int Minimax(char[,] board, List<(int r, int c, char symbol)> order, int depth, bool isMaximizing, int alpha, int beta, char aiPlayer, char humanPlayer)
	{
		if (CheckWin(board, aiPlayer))
		{
			return 100 - depth; // prefer faster wins
		}

		if (CheckWin(board, humanPlayer))
		{
			return depth - 100; // prefer slower losses
		}

		var free = GetFreeCells(board);

		if (free.Count == 0 || depth >= MaxSearchDepth)
		{
			return Evaluate(board, aiPlayer, humanPlayer);
		}

		char mover = isMaximizing ? aiPlayer : humanPlayer;

		if (isMaximizing)
		{
			int best = int.MinValue;

			foreach (var move in free)
			{
				var removed = ApplyMove(order, board, move.r, move.c, mover);
				int score = Minimax(board, order, depth + 1, false, alpha, beta, aiPlayer, humanPlayer);
				UndoMove(order, board, move.r, move.c, removed);

				best = Math.Max(best, score);
				alpha = Math.Max(alpha, best);

				if (beta <= alpha)
				{
					break;
				}
			}

			return best;
		}
		else
		{
			int best = int.MaxValue;

			foreach (var move in free)
			{
				var removed = ApplyMove(order, board, move.r, move.c, mover);
				int score = Minimax(board, order, depth + 1, true, alpha, beta, aiPlayer, humanPlayer);
				UndoMove(order, board, move.r, move.c, removed);

				best = Math.Min(best, score);
				beta = Math.Min(beta, best);

				if (beta <= alpha)
				{
					break;
				}
			}

			return best;
		}
	}

	/// <summary>
	/// Heuristic score for positions beyond the search depth: rewards lines the AI
	/// still has a chance to complete, penalizes lines the opponent could complete,
	/// and ignores lines that are already blocked by both players.
	/// </summary>
	private static int Evaluate(char[,] board, char aiPlayer, char humanPlayer)
	{
		int score = 0;

		foreach (var line in GetLines())
		{
			int aiCount = 0, humanCount = 0;

			foreach (var (r, c) in line)
			{
				if (board[r, c] == aiPlayer)
				{
					aiCount++;
				}
				else if (board[r, c] == humanPlayer)
				{
					humanCount++;
				}
			}

			if (aiCount > 0 && humanCount > 0)
			{
				continue; // blocked line, worth nothing
			}

			if (aiCount > 0)
			{
				score += Weight(aiCount);
			}
			else if (humanCount > 0)
			{
				score -= Weight(humanCount);
			}
		}

		if (board[1, 1] == aiPlayer)
		{
			score += 3;
		}
		else if (board[1, 1] == humanPlayer)
		{
			score -= 3;
		}

		return score;
	}

	private static int Weight(int count) => count switch
	{
		1 => 1,
		2 => 10,
		_ => 0
	};

	private static IEnumerable<(int r, int c)[]> GetLines()
	{
		for (int r = 0; r < 3; r++)
		{
			yield return new[] { (r, 0), (r, 1), (r, 2) };
		}

		for (int c = 0; c < 3; c++)
		{
			yield return new[] { (0, c), (1, c), (2, c) };
		}

		yield return new[] { (0, 0), (1, 1), (2, 2) };
		yield return new[] { (0, 2), (1, 1), (2, 0) };
	}

	/// <summary>
	/// Applies a move to the simulated board/queue exactly the way MakeMove does
	/// (removing the oldest piece once capacity is reached). Returns the removed
	/// cell (with its original symbol) so the move can be undone.
	/// </summary>
	private static (int r, int c, char symbol)? ApplyMove(List<(int r, int c, char symbol)> order, char[,] board, int row, int col, char player)
	{
		(int r, int c, char symbol)? removed = null;

		if (order.Count >= 6)
		{
			removed = order[0];
			order.RemoveAt(0);
			board[removed.Value.r, removed.Value.c] = '\0';
		}

		board[row, col] = player;
		order.Add((row, col, player));

		return removed;
	}

	private static void UndoMove(List<(int r, int c, char symbol)> order, char[,] board, int row, int col, (int r, int c, char symbol)? removed)
	{
		order.RemoveAt(order.Count - 1);
		board[row, col] = '\0';

		if (removed.HasValue)
		{
			board[removed.Value.r, removed.Value.c] = removed.Value.symbol;
			order.Insert(0, removed.Value);
		}
	}

	private static List<(int r, int c)> GetFreeCells(char[,] board)
	{
		var free = new List<(int r, int c)>();

		for (int r = 0; r < 3; r++)
		{
			for (int c = 0; c < 3; c++)
			{
				if (board[r, c] == '\0')
				{
					free.Add((r, c));
				}
			}
		}

		return free;
	}

	private static char[,] CloneBoard(char[,] source)
	{
		var clone = new char[3, 3];
		Array.Copy(source, clone, source.Length);
		return clone;
	}

	private bool CheckWin(char p) => CheckWin(Board, p);

	private static bool CheckWin(char[,] board, char p)
	{
		for (int r = 0; r < 3; r++)
		{
			if (board[r, 0] == p && board[r, 1] == p && board[r, 2] == p)
			{
				return true;
			}
		}

		for (int c = 0; c < 3; c++)
		{
			if (board[0, c] == p && board[1, c] == p && board[2, c] == p)
			{
				return true;
			}
		}

		if (board[0, 0] == p && board[1, 1] == p && board[2, 2] == p)
		{
			return true;
		}

		return board[0, 2] == p && board[1, 1] == p && board[2, 0] == p;
	}
}