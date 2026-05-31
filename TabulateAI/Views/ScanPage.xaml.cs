using TabulateAI.Drawables;
using TabulateAI.ViewModels;

namespace TabulateAI.Views;

public partial class ScanPage : ContentPage
{
    private readonly ReceiptViewfinderDrawable _drawable = new();
    private IDispatcherTimer? _scanTimer;
    private float _scanLineY;

    public ScanPage(ScanViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        ViewfinderGraphics.Drawable = _drawable;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _scanLineY = 0;
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

    private void OnScanTimerTick(object? sender, EventArgs e)
    {
        _scanLineY += 1.5f;
        if (_scanLineY > 240f)
        {
            _scanLineY = 0;
        }

        _drawable.ScanLineY = _scanLineY;
        ViewfinderGraphics.Invalidate();
    }
}
