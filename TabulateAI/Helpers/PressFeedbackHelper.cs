namespace TabulateAI.Helpers;

public static class PressFeedbackHelper
{
    public static void Attach(View view, double pressedScale = 0.92, double pressedOpacity = 0.78)
    {
        var recognizer = new PointerGestureRecognizer();
        recognizer.PointerPressed += (_, _) => ApplyPressed(view, pressedScale, pressedOpacity);
        recognizer.PointerReleased += (_, _) => Reset(view);
        recognizer.PointerExited += (_, _) => Reset(view);
        view.GestureRecognizers.Add(recognizer);
    }

    private static void ApplyPressed(View view, double scale, double opacity)
    {
        view.Scale = scale;
        view.Opacity = opacity;
    }

    private static void Reset(View view)
    {
        view.Scale = 1;
        view.Opacity = 1;
    }
}
