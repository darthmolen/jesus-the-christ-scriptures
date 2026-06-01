using System.Threading.Tasks;
using SQLite;

namespace JesusTheChrist.Data;

public sealed class AppDatabase
{
    public SQLiteAsyncConnection Connection { get; }

    public AppDatabase(string dbPath)
    {
        SQLitePCL.Batteries_V2.Init(); // ensure the native provider is registered
        Connection = new SQLiteAsyncConnection(dbPath);
    }

    public async Task InitializeAsync()
    {
        await Connection.CreateTableAsync<ReadMark>();
        await Connection.CreateTableAsync<NoteEntry>();
        await Connection.CreateTableAsync<Setting>();
    }
}
