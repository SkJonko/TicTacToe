using Microsoft.Extensions.Logging;
using TicToe.Infinite.Data.Models;
using TicToe.Infinite.Resources.Strings;

namespace TicToe.Infinite.Pages;

public partial class MainPage : ContentPage
{
    private TicToeGameService _game = new();
    private TicToeRepository _ticToeRepository = new(ServiceHelper.GetService<ILogger<TicToeRepository>>());
    private bool _isRunning = false;

    public MainPage()
    {
        InitializeComponent();

        _game.CellRemoved += pos => AnimateRemove(GetCell(pos.r, pos.c));
        _game.CellWillBeRemoved += pos => AnimateBreathe(GetCell(pos.r, pos.c));
    }

    private async void Cell_Tapped(object sender, TappedEventArgs e)
    {
		if (_isRunning)
		{
			return;
		}

		_isRunning = true;

        var border = (Border)sender;
        int r = Grid.GetRow(border);
        int c = Grid.GetColumn(border);

        if (!_game.MakeMove(r, c))
		{
			return;
		}

		AnimatePlace((Label)border.Content, _game.Board[r, c]);

        if (_game.Won)
        {
            await AnimateWinAsync(_game.CurrentPlayer);
            return;
        }

        TurnLabel.Text = string.Format(AppResources.MainPage_PlayersTurn, _game.CurrentPlayer);

        // AI logic
        if (AISwitch.IsToggled && _game.CurrentPlayer == _game.AIPlayer)
		{
			await DoAIMoveAsync();
		}

		_isRunning = false;
    }

    private async Task DoAIMoveAsync()
    {
        await Task.Delay(400);

        var move = _game.GetAIMove();

		if (move is null)
		{
			return;
		}

		var (r, c) = move.Value;
        _game.MakeMove(r, c);

        var cell = GetCell(r, c);
        AnimatePlace((Label)cell.Content, _game.Board[r, c]);

        if (_game.Won)
        {
            await AnimateWinAsync(_game.CurrentPlayer);
            return;
        }

        TurnLabel.Text = string.Format(AppResources.MainPage_PlayersTurn, _game.CurrentPlayer);
    }

    private Border GetCell(int r, int c) =>
        BoardGrid.Children.OfType<Border>().First(b => Grid.GetRow(b) == r && Grid.GetColumn(b) == c);

    private async void AnimatePlace(Label label, char symbol)
    {
        label.Text = symbol.ToString();
        label.Opacity = 0;
        label.Scale = 0.2;

        await Task.WhenAll(
            label.FadeToAsync(1, 180),
            label.ScaleToAsync(1, 180, Easing.SpringOut)
        );
    }

    private async void AnimateRemove(Border cell)
    {
        var label = (Label)cell.Content;
        await label.FadeToAsync(0, 250);
        label.Text = "";
        label.Opacity = 1;
    }

    private async void AnimateBreathe(Border cell)
    {
        var oldCell = cell;

        while (oldCell == cell)
        {
            var label = (Label)cell.Content;
			if (string.IsNullOrEmpty(label.Text))
			{
				return;
			}

			await label.ScaleToAsync(1.2, 200, Easing.CubicInOut);
            await Task.Delay(300);
            await label.ScaleToAsync(1.0, 200, Easing.CubicInOut);
        }
    }

    private async Task AnimateWinAsync(char winner)
    {
        TurnLabel.Text = string.Format(AppResources.MainPage_PlayerWins, winner);
        TurnLabel.TextColor = Colors.Gold;

        for (int i = 0; i < 3; i++)
        {
            await TurnLabel.ScaleToAsync(1.2, 150);
            await TurnLabel.ScaleToAsync(1.0, 150);
        }

        await _ticToeRepository.SaveItemAsync(new Game()
        {
            Winner = winner.ToString(),
            Moves = _game.MovesMade
        });
    }

    private void Reset_Clicked(object sender, EventArgs e)
    {
        ResetGame();
    }

    private void ResetGame()
    {
        _game.Reset();
        TurnLabel.Text = string.Format(AppResources.MainPage_PlayersTurn, "X");

        foreach (var border in BoardGrid.Children.OfType<Border>())
        {
            ((Label)border.Content).Text = "";
        }

        _isRunning = false;
    }
}
