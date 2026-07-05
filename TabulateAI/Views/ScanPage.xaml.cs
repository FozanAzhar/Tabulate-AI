using TabulateAI.Drawables;
using TabulateAI.Helpers;
using TabulateAI.ViewModels;

namespace TabulateAI.Views;

public partial class ScanPage : ContentPage
{
    private const float ViewfinderHeight = 238f;

    private readonly ReceiptViewfinderDrawable _drawable = new();
    private IDispatcherTimer? _scanTimer;
    private float _scanLineY;
    private float _viewfinderTop;
    private float _viewfinderBottom;

    public ScanPage(ScanViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        ViewfinderGraphics.Drawable = _drawable;
        ViewfinderGraphics.SizeChanged += OnViewfinderSizeChanged;

        PressFeedbackHelper.Attach(GalleryButton);
        PressFeedbackHelper.Attach(ManualButton);
        PressFeedbackHelper.Attach(ShutterButton, 0.94);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is ScanViewModel viewModel)
        {
            viewModel.ResetInteractionState();
        }

        UpdateViewfinderBounds();
        _scanLineY = _viewfinderTop;

        _scanTimer = Dispatcher.CreateTimer();
        _scanTimer.Interval = TimeSpan.FromMilliseconds(16);
        _scanTimer.Tick += OnScanTimerTick;
        _scanTimer.Start();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (_scanTimer is not null)
        {
            _scanTimer.Tick -= OnScanTimerTick;
            _scanTimer.Stop();
            _scanTimer = null;
        }
    }

    private void OnViewfinderSizeChanged(object? sender, EventArgs e)
    {
        UpdateViewfinderBounds();
    }

    private void UpdateViewfinderBounds()
    {
        if (ViewfinderGraphics.Height <= 0)
        {
            return;
        }

        _viewfinderTop = (float)(ViewfinderGraphics.Height - ViewfinderHeight) / 2f;
        _viewfinderBottom = _viewfinderTop + ViewfinderHeight;

        if (_scanLineY < _viewfinderTop || _scanLineY > _viewfinderBottom)
        {
            _scanLineY = _viewfinderTop;
        }
    }

    private void OnScanTimerTick(object? sender, EventArgs e)
    {
        if (_viewfinderBottom <= _viewfinderTop)
        {
            UpdateViewfinderBounds();
            return;
        }

        _scanLineY += 1.8f;
        if (_scanLineY > _viewfinderBottom)
        {
            _scanLineY = _viewfinderTop;
        }

        _drawable.ScanLineY = _scanLineY;
        ViewfinderGraphics.Invalidate();
    }
}
