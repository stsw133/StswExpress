using Avalonia.Controls;
using Avalonia.Interactivity;

namespace TestApp.Ava;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainContext _viewModel = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    private void IncrementCount(object? sender, RoutedEventArgs e)
    {
        _viewModel.Increment();
    }
}