using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using TeduMarket.Infrastructure.Identity;

namespace TeduMarket.Infrastructure.Persistence;

/// <summary>
/// Áp quy ước đặt tên DB (conventions §1: bảng/cột snake_case, <c>ix_</c>/<c>ux_</c>) cho bảng của thư viện
/// (Identity, OpenIddict) mà không cần thêm thư viện naming. Bảng của dự án tự đặt tên và giữ nguyên.
/// </summary>
internal static partial class SnakeCaseNaming
{
    private static readonly Dictionary<Type, string> _tableNames = new()
    {
        [typeof(User)] = "users",
        [typeof(Role)] = "roles",
        [typeof(IdentityUserRole<Guid>)] = "user_roles",
        [typeof(IdentityUserClaim<Guid>)] = "user_claims",
        [typeof(IdentityUserLogin<Guid>)] = "user_logins",
        [typeof(IdentityUserToken<Guid>)] = "user_tokens",
        [typeof(IdentityUserPasskey<Guid>)] = "user_passkeys",
        [typeof(IdentityRoleClaim<Guid>)] = "role_claims",
    };

    public static void Apply(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = ResolveTableName(entityType);
            if (tableName is null)
            {
                continue;
            }

            entityType.SetTableName(tableName);
            foreach (var property in entityType.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.Name));
            }

            var primaryKey = entityType.FindPrimaryKey();
            primaryKey?.SetName($"pk_{tableName}");

            foreach (var foreignKey in entityType.GetForeignKeys())
            {
                var principalTable = ResolveTableName(foreignKey.PrincipalEntityType) ?? foreignKey.PrincipalEntityType.GetTableName();
                var columns = string.Join('_', foreignKey.Properties.Select(property => ToSnakeCase(property.Name)));
                foreignKey.SetConstraintName($"fk_{tableName}_{principalTable}_{columns}");
            }

            foreach (var index in entityType.GetIndexes())
            {
                var prefix = index.IsUnique ? "ux" : "ix";
                var columns = string.Join('_', index.Properties.Select(property => ToSnakeCase(property.Name)));
                index.SetDatabaseName($"{prefix}_{tableName}_{columns}");
            }
        }
    }

    internal static string ToSnakeCase(string name) => WordBoundary().Replace(name, "_$1").ToLowerInvariant();

    private static string? ResolveTableName(IReadOnlyEntityType entityType)
    {
        if (_tableNames.TryGetValue(entityType.ClrType, out var tableName))
        {
            return tableName;
        }

        // Bảng OpenIddict: OpenIddictEntityFrameworkCoreApplication<...> → openiddict_applications.
        var clrName = entityType.ClrType.Name;
        const string prefix = "OpenIddictEntityFrameworkCore";
        if (!clrName.StartsWith(prefix, StringComparison.Ordinal))
        {
            return null;
        }

        var suffix = clrName[prefix.Length..];
        var withoutArity = suffix.Contains('`', StringComparison.Ordinal) ? suffix[..suffix.IndexOf('`', StringComparison.Ordinal)] : suffix;
        return $"openiddict_{ToSnakeCase(withoutArity)}s";
    }

    [GeneratedRegex("(?<=[a-z0-9])([A-Z])")]
    private static partial Regex WordBoundary();
}
