using System.Security.Cryptography;

namespace Notenokand.Web.Services;

public sealed record StoredBuildingPhoto(string StorageKey, string OriginalFileName, string ContentType, long SizeBytes, string Sha256);

public sealed class BuildingPhotoStorage(IWebHostEnvironment environment)
{
    public const long MaxFileSize = 5 * 1024 * 1024;

    public string? Validate(IFormFile? file)
    {
        if (file is null || file.Length == 0) return null;
        if (file.Length > MaxFileSize) return "รูปตึกต้องมีขนาดไม่เกิน 5 MB";
        var extension = DetectExtension(file);
        return extension is null ? "รองรับเฉพาะรูป JPG, PNG หรือ WEBP" : null;
    }

    public async Task<StoredBuildingPhoto> SaveAsync(IFormFile file, Guid accountId, Guid buildingId)
    {
        await using var input = file.OpenReadStream();
        using var memory = new MemoryStream();
        await input.CopyToAsync(memory);
        var bytes = memory.ToArray();
        var detected = Detect(bytes) ?? throw new InvalidOperationException("Unsupported building photo format.");
        var fileName = Guid.NewGuid().ToString("N") + detected.Extension;
        var storageKey = Path.Combine(accountId.ToString("N"), buildingId.ToString("N"), fileName).Replace(Path.DirectorySeparatorChar, '/');
        var physicalPath = ResolvePath(storageKey);
        Directory.CreateDirectory(Path.GetDirectoryName(physicalPath)!);
        await File.WriteAllBytesAsync(physicalPath, bytes);
        var originalName = Path.GetFileName(file.FileName);
        if (string.IsNullOrWhiteSpace(originalName)) originalName = "building-photo" + detected.Extension;
        if (originalName.Length > 255) originalName = originalName[..255];
        return new StoredBuildingPhoto(storageKey, originalName, detected.ContentType, bytes.LongLength, Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());
    }

    public string ResolvePath(string storageKey)
    {
        var root = Path.GetFullPath(Path.Combine(environment.ContentRootPath, "App_Data", "building-photos"));
        var path = Path.GetFullPath(Path.Combine(root, storageKey.Replace('/', Path.DirectorySeparatorChar)));
        if (!path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Invalid building photo path.");
        return path;
    }

    public void Delete(string? storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey)) return;
        var path = ResolvePath(storageKey);
        if (File.Exists(path)) File.Delete(path);
    }

    private static string? DetectExtension(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        Span<byte> header = stackalloc byte[12];
        var read = stream.Read(header);
        return Detect(header[..read])?.Extension;
    }

    private static (string Extension, string ContentType)? Detect(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF) return (".jpg", "image/jpeg");
        if (bytes.Length >= 8 && bytes[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A })) return (".png", "image/png");
        if (bytes.Length >= 12 && bytes[..4].SequenceEqual("RIFF"u8) && bytes[8..12].SequenceEqual("WEBP"u8)) return (".webp", "image/webp");
        return null;
    }
}