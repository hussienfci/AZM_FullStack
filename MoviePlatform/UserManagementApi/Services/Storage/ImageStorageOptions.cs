namespace UserManagementApi.Services.Storage;

public class ImageStorageOptions
{
    public const string SectionName = "ImageStorage";

    public string RootPath { get; set; } = "App_Data/uploads";

    public long MaxPosterBytes { get; set; } = 5 * 1024 * 1024;
}
