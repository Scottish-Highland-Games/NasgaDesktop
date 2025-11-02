using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NasgaDesktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            // --- Configuration (replace placeholders) ---
            // Remote SQL Server connection string (example). Replace with secure config in production.
            string remoteConnectionString = "Server='shg-database-dev.cx6uiau2aeks.us-east-2.rds.amazonaws.com';Database='NASGA';User Id=admin;Password='GWhwSHpfXxZCT0LLGDQ4';TrustServerCertificate=True;";
            // Local SQLite file path
            string localSqliteConnectionString = "Data Source=D:\\josh\\NASGA\\NasgaDesktop\\my_database.db";

            // Remote query: adapt columns and table name to your remote schema
            string remoteQuery = "SELECT id, name, location, date, inserted_at, updated_at, slug, source FROM Games";

            try
            {
                int syncedCount = 0;

                using (var remoteConn = new SqlConnection(remoteConnectionString))
                {
                    remoteConn.Open();
                    using (var cmd = new SqlCommand(remoteQuery, remoteConn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        using (var localConn = new SqliteConnection(localSqliteConnectionString))
                        {
                            localConn.Open();

                            // Ensure local table exists. Adapt schema as needed.
                            string createTableSql = @"
CREATE TABLE IF NOT EXISTS Games (
    id INTEGER PRIMARY KEY,
    name TEXT,
    location TEXT,
    date TEXT,
    inserted_at TEXT,
    updated_at TEXT,
    slug TEXT,
    source TEXT
);";
                            using (var createCmd = new SqliteCommand(createTableSql, localConn))
                            {
                                createCmd.ExecuteNonQuery();
                            }

                            using (var transaction = localConn.BeginTransaction())
                            {
                                // Upsert command using SQLite ON CONFLICT clause.
                                string upsertSql = @"
INSERT INTO Games (id, name, location, date, inserted_at, updated_at, slug, source)
VALUES (@id, @name, @location, @date, @inserted_at, @updated_at, @slug, @source)
ON CONFLICT(id) DO UPDATE SET
    name = excluded.name,
    location = excluded.location,
    date = excluded.date,
    inserted_at = excluded.inserted_at,
    updated_at = excluded.updated_at,
    slug = excluded.slug,
    source = excluded.source;
";
                                using (var upsertCmd = new SqliteCommand(upsertSql, localConn, transaction))
                                {
                                    // Prepare parameters once and reuse
                                    var pId = upsertCmd.CreateParameter();
                                    pId.ParameterName = "@id";
                                    upsertCmd.Parameters.Add(pId);

                                    var pName = upsertCmd.CreateParameter();
                                    pName.ParameterName = "@name";
                                    upsertCmd.Parameters.Add(pName);

                                    var pLocation = upsertCmd.CreateParameter();
                                    pLocation.ParameterName = "@location";
                                    upsertCmd.Parameters.Add(pLocation);

                                    var pDate = upsertCmd.CreateParameter();
                                    pDate.ParameterName = "@date";
                                    upsertCmd.Parameters.Add(pDate);

                                    var pInserted = upsertCmd.CreateParameter();
                                    pInserted.ParameterName = "@inserted_at";
                                    upsertCmd.Parameters.Add(pInserted);

                                    var pUpdated = upsertCmd.CreateParameter();
                                    pUpdated.ParameterName = "@updated_at";
                                    upsertCmd.Parameters.Add(pUpdated);

                                    var pSlug = upsertCmd.CreateParameter();
                                    pSlug.ParameterName = "@slug";
                                    upsertCmd.Parameters.Add(pSlug);

                                    var pSource = upsertCmd.CreateParameter();
                                    pSource.ParameterName = "@source";
                                    upsertCmd.Parameters.Add(pSource);

                                    // Iterate remote rows and upsert into local SQLite
                                    while (reader.Read())
                                    {
                                        // RemoteQuery: id(0), name(1), location(2), date(3), inserted_at(4), updated_at(5), slug(6), source(7)
                                        int id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                                        string name = reader.IsDBNull(1) ? null : reader.GetString(1);
                                        string location = reader.IsDBNull(2) ? null : reader.GetString(2);

                                        string date;
                                        if (reader.IsDBNull(3))
                                        {
                                            date = null;
                                        }
                                        else
                                        {
                                            var obj = reader.GetValue(3);
                                            if (obj is DateTime dt) date = dt.ToString("o");
                                            else date = obj.ToString();
                                        }

                                        string inserted_at;
                                        if (reader.IsDBNull(4))
                                        {
                                            inserted_at = null;
                                        }
                                        else
                                        {
                                            var obj = reader.GetValue(4);
                                            if (obj is DateTime dt) inserted_at = dt.ToString("o");
                                            else inserted_at = obj.ToString();
                                        }

                                        string updated_at;
                                        if (reader.IsDBNull(5))
                                        {
                                            updated_at = null;
                                        }
                                        else
                                        {
                                            var obj = reader.GetValue(5);
                                            if (obj is DateTime dt) updated_at = dt.ToString("o");
                                            else updated_at = obj.ToString();
                                        }

                                        string slug = reader.IsDBNull(6) ? null : reader.GetString(6);
                                        string source = reader.IsDBNull(7) ? null : reader.GetString(7);

                                        pId.Value = id;
                                        pName.Value = (object?)name ?? DBNull.Value;
                                        pLocation.Value = (object?)location ?? DBNull.Value;
                                        pDate.Value = (object?)date ?? DBNull.Value;
                                        pInserted.Value = (object?)inserted_at ?? DBNull.Value;
                                        pUpdated.Value = (object?)updated_at ?? DBNull.Value;
                                        pSlug.Value = (object?)slug ?? DBNull.Value;
                                        pSource.Value = (object?)source ?? DBNull.Value;

                                        upsertCmd.ExecuteNonQuery();
                                        syncedCount++;
                                    }
                                }

                                transaction.Commit();
                            } // transaction
                        } // localConn
                    } // reader
                } // remoteConn

                // Refresh UI grid if you have one wired up (Button_Click_5 loads the grid)
                try
                {
                    Button_Click_5(null, null);
                }
                catch
                {
                    // ignore if UI refresh not available
                }

                MessageBox.Show($"Sync complete. {syncedCount} rows synchronized.", "Sync Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"Remote database error: {sqlEx.Message}", "Remote DB Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (SqliteException sqliteEx)
            {
                MessageBox.Show($"Local SQLite error: {sqliteEx.Message}", "SQLite Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Unexpected error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            string LocalSqliteConnectionString = "Data Source=D:\\josh\\NASGA\\NasgaDesktop\\my_database.db";

            try
            {
                using (var conn = new SqliteConnection(LocalSqliteConnectionString))
                {
                    conn.Open();

                    // Adjust column list to match your Games table if you want specific columns.
                    string query = "SELECT * FROM Games;";

                    using (var cmd = new SqliteCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        var dt = new DataTable();
                        dt.Load(reader); // DataTable.Load(IDataReader) loads schema + rows

                        // Bind DataTable DefaultView to DataGrid
                        GamesDataGrid.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (SqliteException sqlex)
            {
                MessageBox.Show($"SQLite error while loading games: {sqlex.Message}", "SQLite Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Unexpected error while loading games: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            // Push local changes to cloud when local.updated_at is newer than remote.updated_at
            string localSqliteConnectionString = "Data Source=D:\\josh\\NASGA\\NasgaDesktop\\my_database.db";
            string remoteConnectionString = "Server='shg-database-dev.cx6uiau2aeks.us-east-2.rds.amazonaws.com';Database='NASGA';User Id=admin;Password='GWhwSHpfXxZCT0LLGDQ4';TrustServerCertificate=True;";

            int pushed = 0;
            var localRows = new List<(int id, string name, string location, DateTime? date, DateTime? inserted_at, DateTime? updated_at, string slug, string source)>();

            try
            {
                // 1) Read local rows that have an updated_at value
                using (var localConn = new SqliteConnection(localSqliteConnectionString))
                {
                    localConn.Open();

                    string selectLocal = "SELECT id, name, location, date, inserted_at, updated_at, slug, source FROM Games WHERE updated_at IS NOT NULL;";
                    using (var cmd = new SqliteCommand(selectLocal, localConn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                            string name = reader.IsDBNull(1) ? null : reader.GetString(1);
                            string location = reader.IsDBNull(2) ? null : reader.GetString(2);

                            DateTime? date = null;
                            if (!reader.IsDBNull(3))
                            {
                                DateTime parsed;
                                if (DateTime.TryParse(reader.GetString(3), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out parsed))
                                    date = parsed;
                                else if (DateTime.TryParse(reader.GetValue(3).ToString(), out parsed))
                                    date = parsed;
                            }

                            DateTime? inserted = null;
                            if (!reader.IsDBNull(4))
                            {
                                DateTime parsed;
                                if (DateTime.TryParse(reader.GetString(4), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out parsed))
                                    inserted = parsed;
                                else if (DateTime.TryParse(reader.GetValue(4).ToString(), out parsed))
                                    inserted = parsed;
                            }

                            DateTime? updated = null;
                            if (!reader.IsDBNull(5))
                            {
                                DateTime parsed;
                                if (DateTime.TryParse(reader.GetString(5), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out parsed))
                                    updated = parsed;
                                else if (DateTime.TryParse(reader.GetValue(5).ToString(), out parsed))
                                    updated = parsed;
                            }

                            string slug = reader.IsDBNull(6) ? null : reader.GetString(6);
                            string source = reader.IsDBNull(7) ? null : reader.GetString(7);

                            localRows.Add((id, name, location, date, inserted, updated, slug, source));
                        }
                    }
                }

                if (localRows.Count == 0)
                {
                    MessageBox.Show("No local rows with an updated_at timestamp to consider.", "Nothing to Push", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 2) For each local row, check remote.updated_at and push when local is newer or remote missing
                using (var remoteConn = new SqlConnection(remoteConnectionString))
                {
                    remoteConn.Open();

                    // Prepare commands to reuse parameters
                    using (var getUpdatedCmd = new SqlCommand("SELECT updated_at FROM Games WHERE id = @id", remoteConn))
                    using (var existsCmd = new SqlCommand("SELECT COUNT(1) FROM Games WHERE id = @id", remoteConn))
                    using (var insertWithIdCmd = new SqlCommand("INSERT INTO Games (id, name, location, date, inserted_at, updated_at, slug, source) VALUES (@id, @name, @location, @date, @inserted_at, @updated_at, @slug, @source);", remoteConn))
                    using (var insertNoIdCmd = new SqlCommand("INSERT INTO Games (name, location, date, inserted_at, updated_at, slug, source) VALUES (@name, @location, @date, @inserted_at, @updated_at, @slug, @source); SELECT SCOPE_IDENTITY();", remoteConn))
                    using (var updateRemoteCmd = new SqlCommand(@"UPDATE Games
SET name = @name,
    location = @location,
    date = @date,
    inserted_at = @inserted_at,
    updated_at = @updated_at,
    slug = @slug,
    source = @source
WHERE id = @id AND (@updated_at > ISNULL(updated_at, '1900-01-01'));", remoteConn))
                    {
                        getUpdatedCmd.Parameters.Add(new SqlParameter("@id", System.Data.SqlDbType.Int));
                        existsCmd.Parameters.Add(new SqlParameter("@id", System.Data.SqlDbType.Int));

                        insertWithIdCmd.Parameters.Add(new SqlParameter("@id", System.Data.SqlDbType.Int));
                        insertWithIdCmd.Parameters.Add(new SqlParameter("@name", System.Data.SqlDbType.NVarChar, 4000));
                        insertWithIdCmd.Parameters.Add(new SqlParameter("@location", System.Data.SqlDbType.NVarChar, 4000));
                        insertWithIdCmd.Parameters.Add(new SqlParameter("@date", System.Data.SqlDbType.DateTime));
                        insertWithIdCmd.Parameters.Add(new SqlParameter("@inserted_at", System.Data.SqlDbType.DateTime));
                        insertWithIdCmd.Parameters.Add(new SqlParameter("@updated_at", System.Data.SqlDbType.DateTime));
                        insertWithIdCmd.Parameters.Add(new SqlParameter("@slug", System.Data.SqlDbType.NVarChar, 4000));
                        insertWithIdCmd.Parameters.Add(new SqlParameter("@source", System.Data.SqlDbType.NVarChar, 4000));

                        insertNoIdCmd.Parameters.Add(new SqlParameter("@name", System.Data.SqlDbType.NVarChar, 4000));
                        insertNoIdCmd.Parameters.Add(new SqlParameter("@location", System.Data.SqlDbType.NVarChar, 4000));
                        insertNoIdCmd.Parameters.Add(new SqlParameter("@date", System.Data.SqlDbType.DateTime));
                        insertNoIdCmd.Parameters.Add(new SqlParameter("@inserted_at", System.Data.SqlDbType.DateTime));
                        insertNoIdCmd.Parameters.Add(new SqlParameter("@updated_at", System.Data.SqlDbType.DateTime));
                        insertNoIdCmd.Parameters.Add(new SqlParameter("@slug", System.Data.SqlDbType.NVarChar, 4000));
                        insertNoIdCmd.Parameters.Add(new SqlParameter("@source", System.Data.SqlDbType.NVarChar, 4000));

                        updateRemoteCmd.Parameters.Add(new SqlParameter("@name", System.Data.SqlDbType.NVarChar, 4000));
                        updateRemoteCmd.Parameters.Add(new SqlParameter("@location", System.Data.SqlDbType.NVarChar, 4000));
                        updateRemoteCmd.Parameters.Add(new SqlParameter("@date", System.Data.SqlDbType.DateTime));
                        updateRemoteCmd.Parameters.Add(new SqlParameter("@inserted_at", System.Data.SqlDbType.DateTime));
                        updateRemoteCmd.Parameters.Add(new SqlParameter("@updated_at", System.Data.SqlDbType.DateTime));
                        updateRemoteCmd.Parameters.Add(new SqlParameter("@slug", System.Data.SqlDbType.NVarChar, 4000));
                        updateRemoteCmd.Parameters.Add(new SqlParameter("@source", System.Data.SqlDbType.NVarChar, 4000));
                        updateRemoteCmd.Parameters.Add(new SqlParameter("@id", System.Data.SqlDbType.Int));

                        // Reuse parameter objects to avoid repeated allocations
                        foreach (var r in localRows)
                        {
                            if (!r.updated_at.HasValue)
                                continue;

                            // check existence and remote updated_at
                            existsCmd.Parameters["@id"].Value = r.id;
                            var existsScalar = existsCmd.ExecuteScalar();
                            int existsCount = (existsScalar == null || existsScalar == DBNull.Value) ? 0 : Convert.ToInt32(existsScalar);

                            DateTime? remoteUpdated = null;
                            if (existsCount > 0)
                            {
                                getUpdatedCmd.Parameters["@id"].Value = r.id;
                                var remoteUpdatedObj = getUpdatedCmd.ExecuteScalar();
                                if (remoteUpdatedObj != null && remoteUpdatedObj != DBNull.Value)
                                {
                                    if (remoteUpdatedObj is DateTime dt) remoteUpdated = dt;
                                    else if (DateTime.TryParse(remoteUpdatedObj.ToString(), out var parsed)) remoteUpdated = parsed;
                                }
                            }

                            // Only push when remote missing OR local.updated_at is strictly newer than remote.updated_at
                            bool shouldPush = (existsCount == 0) || (r.updated_at.Value > (remoteUpdated ?? DateTime.MinValue));
                            if (!shouldPush)
                                continue;

                            if (existsCount == 0)
                            {
                                // Insert: if local has a positive id we attempt to insert that id (requires permission).
                                if (r.id > 0)
                                {
                                    bool identityEnabled = false;
                                    try
                                    {
                                        using (var enable = new SqlCommand("SET IDENTITY_INSERT dbo.Games ON;", remoteConn))
                                            enable.ExecuteNonQuery();

                                        identityEnabled = true;

                                        insertWithIdCmd.Parameters["@id"].Value = r.id;
                                        insertWithIdCmd.Parameters["@name"].Value = (object?)r.name ?? DBNull.Value;
                                        insertWithIdCmd.Parameters["@location"].Value = (object?)r.location ?? DBNull.Value;
                                        insertWithIdCmd.Parameters["@date"].Value = (object?)r.date ?? DBNull.Value;
                                        insertWithIdCmd.Parameters["@inserted_at"].Value = (object?)r.inserted_at ?? DBNull.Value;
                                        insertWithIdCmd.Parameters["@updated_at"].Value = (object?)r.updated_at ?? DBNull.Value;
                                        insertWithIdCmd.Parameters["@slug"].Value = (object?)r.slug ?? DBNull.Value;
                                        insertWithIdCmd.Parameters["@source"].Value = (object?)r.source ?? DBNull.Value;

                                        insertWithIdCmd.ExecuteNonQuery();
                                        pushed++;
                                    }
                                    finally
                                    {
                                        if (identityEnabled)
                                        {
                                            try
                                            {
                                                using (var disable = new SqlCommand("SET IDENTITY_INSERT dbo.Games OFF;", remoteConn))
                                                    disable.ExecuteNonQuery();
                                            }
                                            catch
                                            {
                                                // swallow - outer catch will inform user if needed
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // Insert without id; server assigns identity and we read it back
                                    insertNoIdCmd.Parameters["@name"].Value = (object?)r.name ?? DBNull.Value;
                                    insertNoIdCmd.Parameters["@location"].Value = (object?)r.location ?? DBNull.Value;
                                    insertNoIdCmd.Parameters["@date"].Value = (object?)r.date ?? DBNull.Value;
                                    insertNoIdCmd.Parameters["@inserted_at"].Value = (object?)r.inserted_at ?? DBNull.Value;
                                    insertNoIdCmd.Parameters["@updated_at"].Value = (object?)r.updated_at ?? DBNull.Value;
                                    insertNoIdCmd.Parameters["@slug"].Value = (object?)r.slug ?? DBNull.Value;
                                    insertNoIdCmd.Parameters["@source"].Value = (object?)r.source ?? DBNull.Value;

                                    var newIdObj = insertNoIdCmd.ExecuteScalar();
                                    int newId = 0;
                                    if (newIdObj != null && newIdObj != DBNull.Value)
                                        newId = Convert.ToInt32(newIdObj);

                                    // best-effort: update local sqlite id if it had a placeholder id
                                    if (r.id != 0 && newId != 0)
                                    {
                                        using (var localConn2 = new SqliteConnection(localSqliteConnectionString))
                                        {
                                            localConn2.Open();
                                            using (var upd = new SqliteCommand("UPDATE Games SET id = @newId WHERE id = @oldId;", localConn2))
                                            {
                                                upd.Parameters.AddWithValue("@newId", newId);
                                                upd.Parameters.AddWithValue("@oldId", r.id);
                                                upd.ExecuteNonQuery();
                                            }
                                        }
                                    }

                                    pushed++;
                                }
                            }
                            else
                            {
                                // Update remote only when local.updated_at is newer.
                                // Server-side WHERE clause adds an extra safety so we don't overwrite a remote row
                                // that changed after we read updated_at.
                                updateRemoteCmd.Parameters["@name"].Value = (object?)r.name ?? DBNull.Value;
                                updateRemoteCmd.Parameters["@location"].Value = (object?)r.location ?? DBNull.Value;
                                updateRemoteCmd.Parameters["@date"].Value = (object?)r.date ?? DBNull.Value;
                                updateRemoteCmd.Parameters["@inserted_at"].Value = (object?)r.inserted_at ?? DBNull.Value;
                                updateRemoteCmd.Parameters["@updated_at"].Value = (object?)r.updated_at ?? DBNull.Value;
                                updateRemoteCmd.Parameters["@slug"].Value = (object?)r.slug ?? DBNull.Value;
                                updateRemoteCmd.Parameters["@source"].Value = (object?)r.source ?? DBNull.Value;
                                updateRemoteCmd.Parameters["@id"].Value = r.id;

                                int affected = updateRemoteCmd.ExecuteNonQuery();
                                // affected == 0 could mean remote had a newer timestamp or concurrent change — ignore
                                if (affected > 0) pushed++;
                            }
                        }
                    }
                }

                // refresh grid
                try { Button_Click_5(null, null); } catch { }

                MessageBox.Show($"Push complete. {pushed} local changes sent to cloud.", "Push Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"Remote database error: {sqlEx.Message}", "Remote DB Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (SqliteException sqliteEx)
            {
                MessageBox.Show($"Local SQLite error: {sqliteEx.Message}", "SQLite Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error while pushing local changes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            string localSqliteConnectionString = "Data Source=D:\\josh\\NASGA\\NasgaDesktop\\my_database.db";

            // Collect DataRowView entries from the grid
            var rows = new List<DataRowView>();
            foreach (var item in GamesDataGrid.Items)
            {
                if (item is DataRowView drv)
                    rows.Add(drv);
            }

            if (rows.Count == 0)
            {
                MessageBox.Show("No rows available in the grid to save.", "Nothing to Save", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            int savedCount = 0;
            try
            {
                using (var conn = new SqliteConnection(localSqliteConnectionString))
                {
                    conn.Open();

                    // Ensure table exists (same schema used elsewhere)
                    string createTableSql = @"
CREATE TABLE IF NOT EXISTS Games (
    id INTEGER PRIMARY KEY,
    name TEXT,
    location TEXT,
    date TEXT,
    inserted_at TEXT,
    updated_at TEXT,
    slug TEXT,
    source TEXT
);";
                    using (var createCmd = new SqliteCommand(createTableSql, conn))
                        createCmd.ExecuteNonQuery();

                    using (var tran = conn.BeginTransaction())
                    {
                        // Upsert for rows that have a positive id
                        string upsertSql = @"
INSERT INTO Games (id, name, location, date, inserted_at, updated_at, slug, source)
VALUES (@id, @name, @location, @date, @inserted_at, @updated_at, @slug, @source)
ON CONFLICT(id) DO UPDATE SET
    name = excluded.name,
    location = excluded.location,
    date = excluded.date,
    inserted_at = excluded.inserted_at,
    updated_at = excluded.updated_at,
    slug = excluded.slug,
    source = excluded.source;
";
                        using (var upsertCmd = new SqliteCommand(upsertSql, conn, tran))
                        {
                            // Prepare parameters for upsert (id-aware)
                            var pId = upsertCmd.CreateParameter(); pId.ParameterName = "@id"; upsertCmd.Parameters.Add(pId);
                            var pName = upsertCmd.CreateParameter(); pName.ParameterName = "@name"; upsertCmd.Parameters.Add(pName);
                            var pLocation = upsertCmd.CreateParameter(); pLocation.ParameterName = "@location"; upsertCmd.Parameters.Add(pLocation);
                            var pDate = upsertCmd.CreateParameter(); pDate.ParameterName = "@date"; upsertCmd.Parameters.Add(pDate);
                            var pInserted = upsertCmd.CreateParameter(); pInserted.ParameterName = "@inserted_at"; upsertCmd.Parameters.Add(pInserted);
                            var pUpdated = upsertCmd.CreateParameter(); pUpdated.ParameterName = "@updated_at"; upsertCmd.Parameters.Add(pUpdated);
                            var pSlug = upsertCmd.CreateParameter(); pSlug.ParameterName = "@slug"; upsertCmd.Parameters.Add(pSlug);
                            var pSource = upsertCmd.CreateParameter(); pSource.ParameterName = "@source"; upsertCmd.Parameters.Add(pSource);

                            // Insert for rows without id (let SQLite assign rowid)
                            string insertNoIdSql = @"
INSERT INTO Games (name, location, date, inserted_at, updated_at, slug, source)
VALUES (@n_name, @n_location, @n_date, @n_inserted_at, @n_updated_at, @n_slug, @n_source);
";
                            using (var insertCmd = new SqliteCommand(insertNoIdSql, conn, tran))
                            {
                                insertCmd.Parameters.Add(new SqliteParameter("@n_name", DbType.String));
                                insertCmd.Parameters.Add(new SqliteParameter("@n_location", DbType.String));
                                insertCmd.Parameters.Add(new SqliteParameter("@n_date", DbType.String));
                                insertCmd.Parameters.Add(new SqliteParameter("@n_inserted_at", DbType.String));
                                insertCmd.Parameters.Add(new SqliteParameter("@n_updated_at", DbType.String));
                                insertCmd.Parameters.Add(new SqliteParameter("@n_slug", DbType.String));
                                insertCmd.Parameters.Add(new SqliteParameter("@n_source", DbType.String));

                                foreach (var drv in rows)
                                {
                                    // Read fields defensively from the DataRowView
                                    object idObj = drv.Row.Table.Columns.Contains("id") ? drv["id"] : DBNull.Value;
                                    int id = 0;
                                    if (idObj != null && idObj != DBNull.Value)
                                    {
                                        if (!Int32.TryParse(idObj.ToString(), out id))
                                            id = 0;
                                    }

                                    string name = drv.Row.Table.Columns.Contains("name") && drv["name"] != DBNull.Value ? drv["name"].ToString() : null;
                                    string location = drv.Row.Table.Columns.Contains("location") && drv["location"] != DBNull.Value ? drv["location"].ToString() : null;
                                    string date = drv.Row.Table.Columns.Contains("date") && drv["date"] != DBNull.Value ? drv["date"].ToString() : null;
                                    string inserted_at = drv.Row.Table.Columns.Contains("inserted_at") && drv["inserted_at"] != DBNull.Value ? drv["inserted_at"].ToString() : null;
                                    string slug = drv.Row.Table.Columns.Contains("slug") && drv["slug"] != DBNull.Value ? drv["slug"].ToString() : null;
                                    string source = drv.Row.Table.Columns.Contains("source") && drv["source"] != DBNull.Value ? drv["source"].ToString() : null;

                                    // updated_at must be set to current time when saving
                                    string now = DateTime.UtcNow.ToString("o");

                                    if (id > 0)
                                    {
                                        // Upsert using explicit id
                                        pId.Value = id;
                                        pName.Value = (object?)name ?? DBNull.Value;
                                        pLocation.Value = (object?)location ?? DBNull.Value;
                                        pDate.Value = (object?)date ?? DBNull.Value;
                                        pInserted.Value = (object?)inserted_at ?? DBNull.Value;
                                        pUpdated.Value = now;
                                        pSlug.Value = (object?)slug ?? DBNull.Value;
                                        pSource.Value = (object?)source ?? DBNull.Value;

                                        upsertCmd.ExecuteNonQuery();

                                        // Update UI value for updated_at
                                        if (drv.Row.Table.Columns.Contains("updated_at"))
                                            drv["updated_at"] = now;

                                        savedCount++;
                                    }
                                    else
                                    {
                                        // Insert without id (let SQLite assign)
                                        insertCmd.Parameters["@n_name"].Value = (object?)name ?? DBNull.Value;
                                        insertCmd.Parameters["@n_location"].Value = (object?)location ?? DBNull.Value;
                                        insertCmd.Parameters["@n_date"].Value = (object?)date ?? DBNull.Value;
                                        insertCmd.Parameters["@n_inserted_at"].Value = (object?)inserted_at ?? DBNull.Value;
                                        insertCmd.Parameters["@n_updated_at"].Value = now;
                                        insertCmd.Parameters["@n_slug"].Value = (object?)slug ?? DBNull.Value;
                                        insertCmd.Parameters["@n_source"].Value = (object?)source ?? DBNull.Value;

                                        insertCmd.ExecuteNonQuery();

                                        // Retrieve last inserted id and write it back to the grid row if possible
                                        //long newId = conn.LastInsertRowId;
                                        //if (newId > 0 && drv.Row.Table.Columns.Contains("id"))
                                        //{
                                        //    drv["id"] = Convert.ToInt32(newId);
                                        //}

                                        if (drv.Row.Table.Columns.Contains("updated_at"))
                                            drv["updated_at"] = now;

                                        savedCount++;
                                    }
                                } // foreach rows
                            } // insertCmd
                        } // upsertCmd

                        tran.Commit();
                    } // transaction
                } // conn

                MessageBox.Show($"Save complete. {savedCount} rows saved to local SQLite.", "Save Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (SqliteException sqliteEx)
            {
                MessageBox.Show($"Local SQLite error while saving: {sqliteEx.Message}", "SQLite Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error while saving changes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}