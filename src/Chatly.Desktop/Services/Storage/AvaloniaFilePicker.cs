using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Chatly.Contracts.Common.Policies;
using Chatly.Desktop.Abstraction.Storage;

namespace Chatly.Desktop.Services.Storage;

[SingletonService(typeof(IFilePicker))]
internal sealed class AvaloniaFilePicker : IFilePicker
{
    public async Task<IStorageFile?> PickProfilePictureAsync()
    {
        var files = await PickAsync(
            "Select Profile Picture",
            false,
            [
                new FilePickerFileType("Image Files")
                {
                    Patterns = ImageContentTypePolicy.ProfilePictureFilePatterns
                }
            ]);

        return files.Count == 1 ? files[0] : null;
    }

    public Task<IReadOnlyList<IStorageFile>> PickFileAsync(bool allowMultiple = false) =>
        PickAsync("Select File", allowMultiple);

    public async Task<IStorageFile?> PickSaveFileAsync(string suggestedFileName)
    {
        var window = GetMainWindow();
        return await window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save File",
            SuggestedFileName = suggestedFileName,
            ShowOverwritePrompt = true
        });
    }

    private static async Task<IReadOnlyList<IStorageFile>> PickAsync(
        string title,
        bool allowMultiple,
        IReadOnlyList<FilePickerFileType>? fileTypes = null)
    {
        var window = GetMainWindow();
        var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = allowMultiple,
            FileTypeFilter = fileTypes
        });

        return files;
    }

    private static Window GetMainWindow() =>
        (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
        ?.MainWindow
        ?? throw new InvalidOperationException("A main window is required to select a file.");
}
