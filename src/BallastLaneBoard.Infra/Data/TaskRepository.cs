using BallastLaneBoard.Application.TaskManagement;
using BallastLaneBoard.Domain.TaskManagement;
using Npgsql;
using NpgsqlTypes;

namespace BallastLaneBoard.Infra.Data;

internal sealed class TaskRepository(NpgsqlConnection connection, NpgsqlTransaction transaction) : ITaskRepository
{
    public async Task AddAsync(TaskItem entity, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO app."TaskItem" ("Id", "Title", "Description", "Status", "DueDate", "UserId", "CreatedAt", "UpdatedAt")
            VALUES ($1, $2, $3, $4, $5, $6, $7, $8)
            """;

        await using var cmd = new NpgsqlCommand(sql, connection, transaction);
        cmd.Parameters.Add(new() { Value = entity.Id, NpgsqlDbType = NpgsqlDbType.Uuid });
        cmd.Parameters.Add(new() { Value = entity.Title, NpgsqlDbType = NpgsqlDbType.Varchar });
        cmd.Parameters.Add(new() { Value = (object?)entity.Description ?? DBNull.Value, NpgsqlDbType = NpgsqlDbType.Varchar });
        cmd.Parameters.Add(new() { Value = entity.Status.ToString(), NpgsqlDbType = NpgsqlDbType.Text });
        cmd.Parameters.Add(new() { Value = entity.DueDate.HasValue ? entity.DueDate.Value : DBNull.Value, NpgsqlDbType = NpgsqlDbType.TimestampTz });
        cmd.Parameters.Add(new() { Value = entity.UserId, NpgsqlDbType = NpgsqlDbType.Uuid });
        cmd.Parameters.Add(new() { Value = entity.CreatedAt, NpgsqlDbType = NpgsqlDbType.TimestampTz });
        cmd.Parameters.Add(new() { Value = entity.UpdatedAt, NpgsqlDbType = NpgsqlDbType.TimestampTz });

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateAsync(TaskItem entity, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE app."TaskItem"
            SET "Title" = $1, "Description" = $2, "Status" = $3, "DueDate" = $4, "UpdatedAt" = $5
            WHERE "Id" = $6
            """;

        await using var cmd = new NpgsqlCommand(sql, connection, transaction);
        cmd.Parameters.Add(new() { Value = entity.Title, NpgsqlDbType = NpgsqlDbType.Varchar });
        cmd.Parameters.Add(new() { Value = (object?)entity.Description ?? DBNull.Value, NpgsqlDbType = NpgsqlDbType.Varchar });
        cmd.Parameters.Add(new() { Value = entity.Status.ToString(), NpgsqlDbType = NpgsqlDbType.Text });
        cmd.Parameters.Add(new() { Value = entity.DueDate.HasValue ? entity.DueDate.Value : DBNull.Value, NpgsqlDbType = NpgsqlDbType.TimestampTz });
        cmd.Parameters.Add(new() { Value = entity.UpdatedAt, NpgsqlDbType = NpgsqlDbType.TimestampTz });
        cmd.Parameters.Add(new() { Value = entity.Id, NpgsqlDbType = NpgsqlDbType.Uuid });

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task RemoveAsync(TaskItem entity, CancellationToken cancellationToken)
    {
        const string sql = """DELETE FROM app."TaskItem" WHERE "Id" = $1""";

        await using var cmd = new NpgsqlCommand(sql, connection, transaction);
        cmd.Parameters.Add(new() { Value = entity.Id, NpgsqlDbType = NpgsqlDbType.Uuid });

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<TaskItem?> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT "Id", "Title", "Description", "Status", "DueDate", "UserId", "CreatedAt", "UpdatedAt"
            FROM app."TaskItem"
            WHERE "Id" = $1
            """;

        await using var cmd = new NpgsqlCommand(sql, connection, transaction);
        cmd.Parameters.Add(new() { Value = id, NpgsqlDbType = NpgsqlDbType.Uuid });

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadTaskItem(reader) : null;
    }

    public async Task<List<TaskItem>> GetAllOrderedAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT "Id", "Title", "Description", "Status", "DueDate", "UserId", "CreatedAt", "UpdatedAt"
            FROM app."TaskItem"
            ORDER BY "CreatedAt" DESC
            """;

        await using var cmd = new NpgsqlCommand(sql, connection, transaction);
        return await ReadListAsync(cmd, cancellationToken);
    }

    public async Task<List<TaskItem>> GetByUserIdOrderedAsync(Guid userId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT "Id", "Title", "Description", "Status", "DueDate", "UserId", "CreatedAt", "UpdatedAt"
            FROM app."TaskItem"
            WHERE "UserId" = $1
            ORDER BY "CreatedAt" DESC
            """;

        await using var cmd = new NpgsqlCommand(sql, connection, transaction);
        cmd.Parameters.Add(new() { Value = userId, NpgsqlDbType = NpgsqlDbType.Uuid });
        return await ReadListAsync(cmd, cancellationToken);
    }

    private static async Task<List<TaskItem>> ReadListAsync(NpgsqlCommand cmd, CancellationToken cancellationToken)
    {
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var items = new List<TaskItem>();
        while (await reader.ReadAsync(cancellationToken))
            items.Add(ReadTaskItem(reader));
        return items;
    }

    private static TaskItem ReadTaskItem(NpgsqlDataReader reader)
    {
        return TaskItem.Rehydrate(
            id: reader.GetGuid(0),
            title: reader.GetString(1),
            description: reader.IsDBNull(2) ? null : reader.GetString(2),
            status: Enum.Parse<TaskItemStatus>(reader.GetString(3)),
            dueDate: reader.IsDBNull(4) ? null : reader.GetFieldValue<DateTimeOffset>(4),
            userId: reader.GetGuid(5),
            createdAt: reader.GetFieldValue<DateTimeOffset>(6),
            updatedAt: reader.GetFieldValue<DateTimeOffset>(7));
    }
}
