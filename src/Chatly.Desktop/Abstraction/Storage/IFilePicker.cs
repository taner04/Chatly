using Avalonia.Platform.Storage;

namespace Chatly.Desktop.Abstraction.Storage;

public interface IFilePicker
{
    Task<IStorageFile?> PickProfilePictureAsync();
    Task<IReadOnlyList<IStorageFile>> PickFileAsync(bool allowMultiple = false);
    Task<IStorageFile?> PickSaveFileAsync(string suggestedFileName);
}