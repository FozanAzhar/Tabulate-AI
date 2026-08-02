using Microsoft.Maui.Controls.Shapes;

namespace TabulateAI.Helpers;

/// <summary>
/// Shows a temporary top banner without permanently replacing page content in a way
/// that breaks Shell tab navigation.
/// </summary>
public static class InAppBannerHelper
{
    private static CancellationTokenSource? _cts;
    private const string HostId = "InAppNotificationHost";
    private const string BannerId = "InAppNotificationBanner";

    public static void Cancel()
    {
        _cts?.Cancel();
    }

    public static async Task ShowAsync(string title, string message, int durationMs = 4500)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            try
            {
                if (Shell.Current?.CurrentPage is not ContentPage page || page.Content is null)
                {
                    return;
                }

                var host = EnsureHost(page);
                RemoveExistingBanner(host);

                var banner = BuildBanner(title, message);
                banner.ClassId = BannerId;
                host.Children.Insert(0, banner);
                Grid.SetRow(banner, 0);

                banner.Opacity = 0;
                banner.TranslationY = -20;
                await Task.WhenAll(
                    banner.FadeTo(1, 160, Easing.CubicOut),
                    banner.TranslateTo(0, 0, 160, Easing.CubicOut));

                try
                {
                    await Task.Delay(durationMs, token);
                }
                catch (TaskCanceledException)
                {
                    return;
                }

                // Only remove if we're still on the same page/host.
                if (!token.IsCancellationRequested
                    && Shell.Current?.CurrentPage == page
                    && page.Content == host
                    && host.Children.Contains(banner))
                {
                    await Task.WhenAll(
                        banner.FadeTo(0, 120, Easing.CubicIn),
                        banner.TranslateTo(0, -12, 120, Easing.CubicIn));
                    host.Children.Remove(banner);
                }
            }
            catch (Exception ex)
            {
                App.WriteCrashLog($"InAppBannerHelper.ShowAsync: {ex}");
            }
        });
    }

    private static Grid EnsureHost(ContentPage page)
    {
        if (page.Content is Grid existing && existing.ClassId == HostId)
        {
            return existing;
        }

        var original = page.Content;
        var host = new Grid { ClassId = HostId };
        host.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        host.RowDefinitions.Add(new RowDefinition(GridLength.Star));

        // Detach original before re-adding to avoid "already has parent" crashes.
        page.Content = null;
        host.Add(original);
        Grid.SetRow(original, 1);
        page.Content = host;
        return host;
    }

    private static void RemoveExistingBanner(Grid host)
    {
        var existing = host.Children
            .OfType<View>()
            .FirstOrDefault(v => v.ClassId == BannerId);

        if (existing is not null)
        {
            host.Children.Remove(existing);
        }
    }

    private static Border BuildBanner(string title, string message)
    {
        var textStack = new VerticalStackLayout
        {
            Spacing = 2,
            Children =
            {
                new Label
                {
                    Text = title,
                    FontFamily = "OpenSansBold",
                    FontSize = 13,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#111827")
                },
                new Label
                {
                    Text = message,
                    FontFamily = "OpenSansRegular",
                    FontSize = 11,
                    TextColor = Color.FromArgb("#4B5563"),
                    LineBreakMode = LineBreakMode.WordWrap
                }
            }
        };

        var content = new Grid
        {
            ColumnDefinitions =
            [
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            ],
            ColumnSpacing = 10
        };
        content.Add(textStack, 0);
        content.Add(new Label
        {
            Text = "OK",
            FontFamily = "OpenSansBold",
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#7C3AED"),
            VerticalOptions = LayoutOptions.Center
        }, 1);

        var banner = new Border
        {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            BackgroundColor = Color.FromArgb("#F5F3FF"),
            Padding = new Thickness(14, 12),
            Margin = new Thickness(12, 8, 12, 0),
            Content = content
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) =>
        {
            _cts?.Cancel();
            if (banner.Parent is Grid host)
            {
                host.Children.Remove(banner);
            }
        };
        banner.GestureRecognizers.Add(tap);
        return banner;
    }
}
