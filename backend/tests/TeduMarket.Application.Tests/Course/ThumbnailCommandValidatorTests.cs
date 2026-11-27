using TeduMarket.Application.Course.Commands.CreateThumbnailUpload;
using TeduMarket.Application.Course.Commands.SetCourseThumbnail;

namespace TeduMarket.Application.Tests.Course;

[Trait("Category", "Unit")]
public sealed class ThumbnailCommandValidatorTests
{
    private const long MaxLength = 2 * 1024 * 1024;

    private static readonly Guid _courseId = Guid.NewGuid();

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("image/webp")]
    [InlineData("IMAGE/PNG")]
    public void Validate_UploadWithAllowedContentType_ReturnsValid(string contentType)
    {
        // Arrange / Act
        var result = new CreateThumbnailUploadCommandValidator().Validate(new CreateThumbnailUploadCommand(_courseId, contentType, 1024));

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("image/gif")]
    [InlineData("image/svg+xml")]
    [InlineData("application/pdf")]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_UploadWithDisallowedContentType_ReturnsErrorOnContentType(string? contentType)
    {
        // Arrange / Act
        var result = new CreateThumbnailUploadCommandValidator().Validate(new CreateThumbnailUploadCommand(_courseId, contentType, 1024));

        // Assert
        Assert.Equal(["ContentType"], result.Errors.Select(error => error.PropertyName));
    }

    [Fact]
    public void Validate_UploadAtMaxLength_ReturnsValid()
    {
        // Arrange / Act
        var result = new CreateThumbnailUploadCommandValidator().Validate(new CreateThumbnailUploadCommand(_courseId, "image/png", MaxLength));

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(MaxLength + 1)]
    [InlineData(0L)]
    [InlineData(-5L)]
    [InlineData(null)]
    public void Validate_UploadWithBadContentLength_ReturnsErrorOnContentLength(long? length)
    {
        // Arrange / Act
        var result = new CreateThumbnailUploadCommandValidator().Validate(new CreateThumbnailUploadCommand(_courseId, "image/png", length));

        // Assert
        Assert.Equal(["ContentLength"], result.Errors.Select(error => error.PropertyName).Distinct());
    }

    [Fact]
    public void Validate_SetThumbnailWithKeyOfSameCourse_ReturnsValid()
    {
        // Arrange
        var key = $"{_courseId:N}/{Guid.NewGuid():N}.webp";

        // Act
        var result = new SetCourseThumbnailCommandValidator().Validate(new SetCourseThumbnailCommand(_courseId, key));

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("../secret.png")]
    [InlineData("khong-dung-dang.png")]
    public void Validate_SetThumbnailWithMalformedKey_ReturnsErrorOnObjectKey(string? key)
    {
        // Arrange / Act
        var result = new SetCourseThumbnailCommandValidator().Validate(new SetCourseThumbnailCommand(_courseId, key));

        // Assert
        Assert.Equal(["ObjectKey"], result.Errors.Select(error => error.PropertyName));
    }

    [Fact]
    public void Validate_SetThumbnailWithKeyOfOtherCourse_ReturnsErrorOnObjectKey()
    {
        // Arrange
        var key = $"{Guid.NewGuid():N}/{Guid.NewGuid():N}.png";

        // Act
        var result = new SetCourseThumbnailCommandValidator().Validate(new SetCourseThumbnailCommand(_courseId, key));

        // Assert
        Assert.Equal(["ObjectKey"], result.Errors.Select(error => error.PropertyName));
    }
}
