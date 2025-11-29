using CommunityToolkit.Mvvm.ComponentModel;

namespace TicToe.Infinite.PageModels;

public partial class BasePageModel : ObservableObject
{
    [ObservableProperty]
    public bool _isBusy = false;
}