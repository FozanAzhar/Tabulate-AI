using TabulateAI.ViewModels;

namespace TabulateAI.Views;

public partial class ProcessingPage : ContentPage
{
    private readonly ProcessingViewModel _viewModel;
    private bool _started;
    private bool _isAnimating;
    private CancellationTokenSource? _animationCts;

    public ProcessingPage(ProcessingViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _animationCts = new CancellationTokenSource();
        _ = RunPulseAnimationAsync(_animationCts.Token);

        if (_started)
        {
            return;
        }

        _started = true;
        await _viewModel.StartProcessingAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _animationCts?.Cancel();
        _animationCts?.Dispose();
        _animationCts = null;
    }

    private async Task RunPulseAnimationAsync(CancellationToken token)
    {
        if (_isAnimating)
        {
            return;
        }

        _isAnimating = true;

        try
        {
            while (!token.IsCancellationRequested && IsVisible)
            {
                await PreviewCard.FadeTo(0.4, 800, Easing.SinInOut);
                if (token.IsCancellationRequested || !IsVisible)
                {
                    break;
                }

                await PreviewCard.FadeTo(1.0, 800, Easing.SinInOut);
            }
        }
        catch (TaskCanceledException)
        {
            // Page closed — expected.
        }
        catch (ObjectDisposedException)
        {
            // Page destroyed mid-animation — expected.
        }
        finally
        {
            _isAnimating = false;
        }
    }
}
