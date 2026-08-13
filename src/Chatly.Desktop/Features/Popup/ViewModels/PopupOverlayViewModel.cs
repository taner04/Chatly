using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chatly.Desktop.Shared.Abstractions;
using Chatly.Desktop.Shared.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Chatly.Desktop.Features.Popup.ViewModels;

public sealed partial class PopupOverlayViewModel : ViewModelBase
{
    private readonly IReadOnlyDictionary<Type, IMessageOverlayRegistration> _registrations;
    private TaskCompletionSource? _closed;

    public PopupOverlayViewModel(IEnumerable<IMessageOverlayRegistration> registrations)
    {
        var registrationsByType = new Dictionary<Type, IMessageOverlayRegistration>();

        foreach (var registration in registrations)
        {
            if (!registrationsByType.TryAdd(registration.OverlayType, registration))
            {
                throw new InvalidOperationException(
                    $"A message overlay of type {registration.OverlayType.Name} is already registered.");
            }
        }

        _registrations = registrationsByType;
    }

    [ObservableProperty]
    public partial bool IsOpen { get; private set; }

    [ObservableProperty]
    public partial string? Title { get; private set; }

    [ObservableProperty]
    public partial object? Content { get; private set; }

    [ObservableProperty]
    public partial bool IsCloseButtonVisible { get; private set; } = true;

    public async Task ShowAsync<T>(
        string title,
        bool isCloseButtonVisible = true) where T : IMessageOverlay
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        if (IsOpen)
        {
            throw new InvalidOperationException("A popup is already open.");
        }

        if (!_registrations.TryGetValue(typeof(T), out var registration))
        {
            throw new InvalidOperationException(
                $"No message overlay of type {typeof(T).Name} is registered.");
        }

        var instance = registration.Create();

        _closed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Title = title;
        Content = instance.View;
        IsCloseButtonVisible = isCloseButtonVisible;
        IsOpen = true;

        try
        {
            await Task.WhenAny(instance.Overlay.Completion, _closed.Task);
        }
        finally
        {
            Close();
        }
    }

    [RelayCommand]
    public void Close()
    {
        if (!IsOpen)
        {
            return;
        }

        IsOpen = false;
        Title = null;
        Content = null;
        IsCloseButtonVisible = true;

        var closed = _closed;
        _closed = null;
        closed?.TrySetResult();
    }
}
