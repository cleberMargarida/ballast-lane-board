using BallastLaneBoard.Application.Identity;
using BallastLaneBoard.Domain.Identity;
using Npgsql;
using NpgsqlTypes;

namespace BallastLaneBoard.Infra.Data;

internal sealed class UserRepository(NpgsqlConnection connection, NpgsqlTransaction transaction) : IUserRepository
{
    public async Task AddAsync(AppUser entity, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO app."AppUser" ("Id", "ExternalSubject", "Username", "Email", "Role", "CreatedAt", "LastSeenAt")
            VALUES ($1, $2, $3, $4, $5, $6, $7)
            """;

        await using var cmd = new NpgsqlCommand(sql, connection, transaction);
        cmd.Parameters.Add(new() { Value = entity.Id, NpgsqlDbType = NpgsqlDbType.Uuid });
        cmd.Parameters.Add(new() { Value = entity.ExternalSubject, NpgsqlDbType = NpgsqlDbType.Varchar });
        cmd.Parameters.Add(new() { Value = entity.Username, NpgsqlDbType = NpgsqlDbType.Varchar });
        cmd.Parameters.Add(new() { Value = entity.Email, NpgsqlDbType = NpgsqlDbType.Varchar });
        cmd.Parameters.Add(new() { Value = entity.Role.ToString(), NpgsqlDbType = NpgsqlDbType.Text });
        cmd.Parameters.Add(new() { Value = entity.CreatedAt, NpgsqlDbType = NpgsqlDbType.TimestampTz });
        cmd.Parameters.Add(new() { Value = entity.LastSeenAt, NpgsqlDbType = NpgsqlDbType.TimestampTz });

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateAsync(AppUser entity, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE app."AppUser"
            SET "Username" = $1, "Email" = $2, "Role" = $3, "LastSeenAt" = $4
            WHERE "Id" = $5
            """;

        await using var cmd = new NpgsqlCommand(sql, connection, transaction);
        cmd.Parameters.Add(new() { Value = entity.Username, NpgsqlDbType = NpgsqlDbType.Varchar });
        cmd.Parameters.Add(new() { Value = entity.Email, NpgsqlDbType = NpgsqlDbType.Varchar });
        cmd.Parameters.Add(new() { Value = entity.Role.ToString(), NpgsqlDbType = NpgsqlDbType.Text });
        cmd.Parameters.Add(new() { Value = entity.LastSeenAt, NpgsqlDbType = NpgsqlDbType.TimestampTz });
        cmd.Parameters.Add(new() { Value = entity.Id, NpgsqlDbType = NpgsqlDbType.Uuid });

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task RemoveAsync(AppUser entity, CancellationToken cancellationToken)
    {
        const string sql = """DELETE FROM app."AppUser" WHERE "Id" = $1""";

        await using var cmd = new NpgsqlCommand(sql, connection, transaction);
        cmd.Parameters.Add(new() { Value = entity.Id, NpgsqlDbType = NpgsqlDbType.Uuid });

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<AppUser?> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT "Id", "ExternalSubject", "Username", "Email", "Role", "CreatedAt", "LastSeenAt"
            FROM app."AppUser"
            WHERE "Id" = $1
            """;

        await using var cmd = new NpgsqlCommand(sql, connection, transaction);
        cmd.Parameters.Add(new() { Value = id, NpgsqlDbType = NpgsqlDbType.Uuid });

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadAppUser(reader) : null;
    }

    public async Task<AppUser?> FindBySubjectAsync(string externalSubject, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT "Id", "ExternalSubject", "Username", "Email", "Role", "CreatedAt", "LastSeenAt"
            FROM app."AppUser"
            WHERE "ExternalSubject" = $1
            """;

        await using var cmd = new NpgsqlCommand(sql, connection, transaction);
        cmd.Parameters.Add(new() { Value = externalSubject, NpgsqlDbType = NpgsqlDbType.Varchar });

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadAppUser(reader) : null;
    }

    private static AppUser ReadAppUser(NpgsqlDataReader reader)
    {
        return AppUser.Reconstitute(
            id: reader.GetGuid(0),
            externalSubject: reader.GetString(1),
            username: reader.GetString(2),
            email: reader.GetString(3),
            role: Enum.Parse<UserRole>(reader.GetString(4)),
            createdAt: reader.GetFieldValue<DateTimeOffset>(5),
            lastSeenAt: reader.GetFieldValue<DateTimeOffset>(6));
    }
}
