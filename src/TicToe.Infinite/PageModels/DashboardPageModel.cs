using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TicToe.Infinite.Models;

namespace TicToe.Infinite.PageModels;

public partial class DashboardPageModel : BasePageModel
{
    private readonly TicToeRepository _repo;

    #region Properties

    [ObservableProperty]
    private ObservableCollection<WinnerInfo> _winnerData = [];

    [ObservableProperty]
    private ObservableCollection<Brush> _winnerColors =
    [
        new SolidColorBrush(Color.FromArgb("#4CAF50")), // X
        new SolidColorBrush(Color.FromArgb("#F44336")), // O
    ];

    [ObservableProperty]
    private int _totalGames;

    #endregion


    public DashboardPageModel(TicToeRepository ticToeRepository)
    {
        _repo = ticToeRepository;
    }


    #region Commands

    [RelayCommand]
    private async Task AppearingAsync()
        => await LoadData();

    #endregion

    private async Task LoadData()
    {
        IsBusy = true;

        await Task.Delay(1000);

        var games = await _repo.ListAsync();

        TotalGames = games.Count;

        WinnerData.Clear();

        foreach (var item in games.GroupBy(x => x.Winner))
        {
            WinnerData.Add(new WinnerInfo(item.Key, item.Count(g => g.Winner == item.Key), item.Where(g => g.Winner == item.Key).Sum(g => g.Moves)));
        }

        IsBusy = false;
    }
}
