using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Notenokand.Web.Services;

namespace Notenokand.Tests;

public sealed class BuildingPhotoStorageTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "notenokand-photo-tests-" + Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task SavesJpegUsingDetectedTypeAndPrivateStorageKey()
    {
        Directory.CreateDirectory(root);
        var service = new BuildingPhotoStorage(new TestEnvironment(root));
        var bytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 1, 2, 3, 4 };
        var file = CreateFile(bytes, "ตึกนก.jpg", "application/octet-stream");

        Assert.Null(service.Validate(file));
        var result = await service.SaveAsync(file, Guid.Parse("11111111-1111-1111-1111-111111111111"), Guid.Parse("22222222-2222-2222-2222-222222222222"));

        Assert.Equal("image/jpeg", result.ContentType);
        Assert.EndsWith(".jpg", result.StorageKey);
        Assert.True(File.Exists(service.ResolvePath(result.StorageKey)));
        Assert.Equal(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes)).ToLowerInvariant(), result.Sha256);
    }

    [Fact]
    public void RejectsFileWhoseBytesAreNotAnAllowedImage()
    {
        var service = new BuildingPhotoStorage(new TestEnvironment(root));
        var file = CreateFile("not an image"u8.ToArray(), "fake.jpg", "image/jpeg");
        Assert.Equal("รองรับเฉพาะรูป JPG, PNG หรือ WEBP", service.Validate(file));
    }

    [Fact]
    public void RejectsStoragePathOutsidePrivateRoot()
    {
        var service = new BuildingPhotoStorage(new TestEnvironment(root));
        Assert.Throws<InvalidOperationException>(() => service.ResolvePath("../../outside.jpg"));
    }

    private static FormFile CreateFile(byte[] bytes, string fileName, string contentType)
    {
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "Photo", fileName) { Headers = new HeaderDictionary(), ContentType = contentType };
    }

    public void Dispose()
    {
        if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
    }

    private sealed class TestEnvironment(string contentRootPath) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Notenokand.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = contentRootPath;
        public string EnvironmentName { get; set; } = "Testing";
        public string ContentRootPath { get; set; } = contentRootPath;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}