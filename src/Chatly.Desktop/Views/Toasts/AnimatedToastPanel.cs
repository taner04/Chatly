using System.Linq;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Media;
using Avalonia.Threading;

namespace Chatly.Desktop.Views.Toasts;

public sealed class AnimatedToastPanel : Panel
{
    private readonly Dictionary<Control, Rect> _previousBounds = [];

    protected override Size MeasureOverride(Size availableSize)
    {
        var width = 0d;
        var height = 0d;

        foreach (var child in Children)
        {
            child.Measure(new Size(availableSize.Width, double.PositiveInfinity));
            width = Math.Max(width, child.DesiredSize.Width);
            height += child.DesiredSize.Height;
        }

        return new Size(width, height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var currentChildren = Children.ToHashSet();
        foreach (var child in _previousBounds.Keys.Where(child => !currentChildren.Contains(child)).ToArray())
        {
            _previousBounds.Remove(child);
        }

        var y = 0d;
        foreach (var child in Children)
        {
            var bounds = new Rect(0, y, finalSize.Width, child.DesiredSize.Height);
            //TODO: Fix possible loss of precision when comparing double values
            if (_previousBounds.TryGetValue(child, out var previousBounds) && previousBounds.Y != bounds.Y)
            {
                AnimateToNewPosition(child, previousBounds.Y - bounds.Y);
            }

            child.Arrange(bounds);
            _previousBounds[child] = bounds;
            y += bounds.Height;
        }

        return finalSize;
    }

    private static void AnimateToNewPosition(Control child, double offset)
    {
        if (child.RenderTransform is not TranslateTransform transform)
        {
            transform = new TranslateTransform
            {
                Transitions =
                [
                    new DoubleTransition
                    {
                        Property = TranslateTransform.YProperty,
                        Duration = TimeSpan.FromMilliseconds(180)
                    }
                ]
            };
            child.RenderTransform = transform;
        }

        transform.Y = offset;
        Dispatcher.UIThread.Post(() => transform.Y = 0, DispatcherPriority.Background);
    }
}