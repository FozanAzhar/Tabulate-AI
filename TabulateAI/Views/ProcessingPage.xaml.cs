using TabulateAI.ViewModels;

namespace TabulateAI.Views;

public partial class ProcessingPage : ContentPage
{
    private readonly ProcessingViewModel _viewModel;
    private bool _started;
    private bool _isPulsing;
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
        _ = RunProgressAnimationAsync(_animationCts.Token);

        if (_started)
        {
            return;
        }

        _started = true;

        // Let Shell query parameters (ImagePath, etc.) bind before processing starts.
        await Task.Delay(100);
        await _viewModel.StartProcessingAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _animationCts?.Cancel();
        _animationCts?.Dispose();
        _animationCts = null;
        _started = false;
    }

    private async Task RunPulseAnimationAsync(CancellationToken token)
    {
        if (_isPulsing)
        {
            return;
        }

        _isPulsing = true;

        try
        {
            while (!token.IsCancellationRequested && IsVisible)
            {
                await ReceiptCard.FadeTo(0.4, 800, Easing.CubicInOut);
                if (token.IsCancellationRequested || !IsVisible)
                {
                    break;
                }

                await ReceiptCard.FadeTo(1.0, 800, Easing.CubicInOut);
            }
        }
        catch (TaskCanceledException)
        {
            // Page closed.
        }
        catch (ObjectDisposedException)
        {
            // Page destroyed mid-animation.
        }
        finally
        {
            _isPulsing = false;
        }
    }

    private async Task RunProgressAnimationAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested && !_viewModel.IsComplete)
            {
                await ProcessingProgressBar.ProgressTo(0.9, 2000, Easing.CubicInOut);
                if (token.IsCancellationRequested || _viewModel.IsComplete)
                {
                    break;
                }

                ProcessingProgressBar.Progress = 0;
            }

            if (_viewModel.IsComplete && !token.IsCancellationRequested)
            {
                await ProcessingProgressBar.ProgressTo(1.0, 400, Easing.CubicInOut);
            }
        }
        catch (TaskCanceledException)
        {
            // Page closed.
        }
        catch (OperationCanceledException)
        {
            // Animation cancelled.
        }
    }
}
