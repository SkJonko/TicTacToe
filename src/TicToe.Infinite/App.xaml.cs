namespace TicToe.Infinite;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell())
        {
            Height = 700,
            Width = 500
        };
    }
}