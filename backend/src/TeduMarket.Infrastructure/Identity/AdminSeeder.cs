using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeduMarket.Domain.Identity;

namespace TeduMarket.Infrastructure.Identity;

/// <summary>
/// Tạo Admin đầu tiên với mật khẩu ngẫu nhiên và ghi mật khẩu vào file bí mật ngoài source control (FR-41, NFR-22).
/// Mật khẩu chỉ được lưu dạng băm trong DB (Identity PBKDF2); chạy lại khi Admin đã tồn tại thì không làm gì.
/// </summary>
public sealed partial class AdminSeeder(
    UserManager<User> userManager,
    IOptions<AdminSeedOptions> options,
    IHostEnvironment environment,
    ILogger<AdminSeeder> logger)
{
    private static readonly JsonSerializerOptions _secretFileJsonOptions = new() { WriteIndented = true };
    private const int _passwordLength = 24;
    private const string _lower = "abcdefghijkmnopqrstuvwxyz";
    private const string _upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string _digits = "23456789";
    private const string _symbols = "!@#$%^*-_=+";

    /// <summary>Trả <c>true</c> nếu vừa tạo Admin, <c>false</c> nếu đã có từ trước.</summary>
    public async Task<bool> SeedAsync(CancellationToken cancellationToken)
    {
        var seedOptions = options.Value;
        if (await userManager.FindByEmailAsync(seedOptions.Email) is not null)
        {
            return false;
        }

        var password = GeneratePassword();
        var secretFilePath = string.IsNullOrWhiteSpace(seedOptions.SecretFilePath)
            ? Path.Combine(environment.ContentRootPath, "secrets", "secret.json")
            : seedOptions.SecretFilePath;

        // Ghi file trước khi tạo tài khoản: nếu ghi lỗi thì chưa có Admin, lần chạy sau thử lại; không bao giờ có Admin mà mất mật khẩu.
        await WriteSecretFileAsync(secretFilePath, seedOptions.Email, password, cancellationToken);

        var user = new User { UserName = seedOptions.Email, Email = seedOptions.Email, EmailConfirmed = true };
        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException($"Không tạo được Admin seed: {string.Join("; ", createResult.Errors.Select(error => error.Code))}");
        }

        var roleResult = await userManager.AddToRoleAsync(user, RoleNames.Admin);
        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException($"Không gán vai trò Admin cho tài khoản seed: {string.Join("; ", roleResult.Errors.Select(error => error.Code))}");
        }

        LogAdminSeeded(seedOptions.Email, secretFilePath);
        return true;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Đã tạo Admin {Email}; mật khẩu ghi vào {SecretFilePath}")]
    private partial void LogAdminSeeded(string email, string secretFilePath);

    private static async Task WriteSecretFileAsync(string path, string email, string password, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var streamOptions = new FileStreamOptions { Mode = FileMode.Create, Access = FileAccess.Write };
        if (!OperatingSystem.IsWindows())
        {
            streamOptions.UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite;
        }

        await using var stream = new FileStream(path, streamOptions);
        await JsonSerializer.SerializeAsync(stream, new { AdminSeed = new { Email = email, Password = password } }, _secretFileJsonOptions, cancellationToken);
    }

    private static string GeneratePassword()
    {
        // Mỗi nhóm ký tự ít nhất 1 ký tự để thỏa chính sách mật khẩu mặc định của Identity; phần còn lại lấy từ cả 4 nhóm.
        var all = _lower + _upper + _digits + _symbols;
        var chars = new List<char>(_passwordLength)
        {
            Pick(_lower), Pick(_upper), Pick(_digits), Pick(_symbols),
        };
        while (chars.Count < _passwordLength)
        {
            chars.Add(Pick(all));
        }

        RandomNumberGenerator.Shuffle(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(chars));
        return new string([.. chars]);
    }

    private static char Pick(string alphabet) => alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
}
