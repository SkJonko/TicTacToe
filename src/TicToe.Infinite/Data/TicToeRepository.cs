using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using TicToe.Infinite.Data.Models;

namespace TicToe.Infinite.Data;

/// <summary>
/// Repository class for managing tags in the database.
/// </summary>
public class TicToeRepository
{
    private bool _hasBeenInitialized = false;
    private readonly ILogger<TicToeRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TicToeRepository"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public TicToeRepository(ILogger<TicToeRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
    }

    /// <summary>
    /// Saves a Game to the database.
    /// </summary>
    /// <param name="item">The Game to save.</param>
    /// <returns>The ID of the saved Game.</returns>
    public async Task<int> SaveItemAsync(Game item)
    {
        await Init();

        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var saveCmd = connection.CreateCommand();

        saveCmd.CommandText = @"
            INSERT INTO Games (Winner, Moves) VALUES (@Winner, @Moves);
            SELECT last_insert_rowid();";

        saveCmd.Parameters.AddWithValue("@Winner", item.Winner);
        saveCmd.Parameters.AddWithValue("@Moves", item.Moves);

        var result = await saveCmd.ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }

    /// <summary>
    /// Retrieves a list of all Games from the database.
    /// </summary>
    /// <returns>A list of <see cref="Game"/> objects.</returns>
    public async Task<List<Game>> ListAsync()
    {
        await Init();

        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var selectCmd = connection.CreateCommand();

        selectCmd.CommandText = "SELECT * FROM Games";

        var games = new List<Game>();

        await using var reader = await selectCmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            games.Add(new Game
            {
                Id = reader.GetInt32(0),
                Winner = reader.GetString(1),
                Moves = reader.GetInt32(0)
            });
        }

        return games;
    }

    /// <summary>
    /// Drops the Tag and ProjectsTags tables from the database.
    /// </summary>
    public async Task DropTableAsync()
    {
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var dropTableCmd = connection.CreateCommand();
        dropTableCmd.CommandText = "DROP TABLE IF EXISTS Game";

        await dropTableCmd.ExecuteNonQueryAsync();

        _hasBeenInitialized = false;
    }

    /// <summary>
    /// Initializes the database connection and creates the Tag and ProjectsTags tables if they do not exist.
    /// </summary>
    private async Task Init()
    {
        if (_hasBeenInitialized)
            return;

        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        try
        {
            var createTableCmd = connection.CreateCommand();

            createTableCmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Games (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                Winner TEXT NOT NULL,
                Moves INTEGER NOT NULL
            );";

            await createTableCmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            //_logger.LogError(ex, "Error creating tables");
            throw;
        }

        _hasBeenInitialized = true;
    }
}