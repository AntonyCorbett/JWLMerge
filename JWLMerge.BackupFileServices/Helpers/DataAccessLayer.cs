using System.Globalization;

namespace JWLMerge.BackupFileServices.Helpers
{
    using Microsoft.Data.Sqlite;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Models.DatabaseModels;
    using Serilog;

    /// <summary>
    /// Isolates all data access to the SQLite database embedded in
    /// jwlibrary files.
    /// </summary>
    internal sealed class DataAccessLayer
    {
        private readonly string _databaseFilePath;

        public DataAccessLayer(string databaseFilePath)
        {
            _databaseFilePath = databaseFilePath;
        }

        /// <summary>
        /// Creates a new empty database using the schema from the current database.
        /// </summary>
        /// <param name="cloneFilePath">The clone file path (the new database).</param>
        public void CreateEmptyClone(string cloneFilePath)
        {
            Log.Logger.Debug($"Creating empty clone: {cloneFilePath}");

            using var source = CreateConnection(_databaseFilePath);
            using var destination = CreateConnection(cloneFilePath);

            source.BackupDatabase(destination, "main", "main");
            ClearData(destination);
        }

        /// <summary>
        /// Populates the current database using the specified data.
        /// </summary>
        /// <param name="dataToUse">The data to use.</param>
        public void PopulateTables(Database dataToUse)
        {
            using var connection = CreateConnection();

            PopulateTable(connection, dataToUse.Locations);
            PopulateTable(connection, dataToUse.UserMarks);
            PopulateTable(connection, dataToUse.Tags);
            PopulateTable(connection, dataToUse.Notes);
            PopulateTable(connection, dataToUse.TagMaps);
            PopulateTable(connection, dataToUse.InputFields);
            PopulateTable(connection, dataToUse.Bookmarks);
            PopulateTable(connection, dataToUse.BlockRanges);
        }

        /// <summary>
        /// Reads the current database.
        /// </summary>
        /// <returns><see cref="Database"/></returns>
        public Database ReadDatabase()
        {
            var result = new Database();

            using var connection = CreateConnection();

            result.InitBlank();

            result.LastModified.TimeLastModified = ReadAllRows(connection, ReadLastModified).FirstOrDefault()?.TimeLastModified;
            result.Locations.AddRange(ReadAllRows(connection, ReadLocation));
            result.Notes.AddRange(ReadAllRows(connection, ReadNote));
            result.Tags.AddRange(ReadAllRows(connection, ReadTag));
            result.TagMaps.AddRange(ReadAllRows(connection, ReadTagMap));
            result.BlockRanges.AddRange(ReadAllRows(connection, ReadBlockRange));
            result.Bookmarks.AddRange(ReadAllRows(connection, ReadBookmark));
            result.UserMarks.AddRange(ReadAllRows(connection, ReadUserMark));
            result.InputFields.AddRange(ReadAllRows(connection, ReadInputField));

            // ensure bookmarks appear in similar order to original.
            result.Bookmarks.Sort((bookmark1, bookmark2) => bookmark1.Slot.CompareTo(bookmark2.Slot));

            return result;
        }

        private static List<TRowType> ReadAllRows<TRowType>(
            SqliteConnection connection,
            Func<SqliteDataReader, TRowType> readRowFunction)
        {
            using var cmd = connection.CreateCommand();

            var result = new List<TRowType>();
            var tableName = typeof(TRowType).Name;

            cmd.CommandText = $"select * from {tableName}";
            Log.Logger.Debug($"SQL: {cmd.CommandText}");
                
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.Add(readRowFunction(reader));
                }
            }

            Log.Logger.Debug($"SQL result set count: {result.Count}");
                
            return result;
        }

        private static string ReadString(SqliteDataReader reader, string columnName)
        {
            return reader[columnName].ToString()!;
        }

        private static string? ReadNullableString(SqliteDataReader reader, string columnName)
        {
            var value = reader[columnName];
            return value == DBNull.Value ? null : value.ToString();
        }

        private static int ReadInt(SqliteDataReader reader, string columnName)
        {
            return Convert.ToInt32(reader[columnName], CultureInfo.InvariantCulture);
        }

        private static int? ReadNullableInt(SqliteDataReader reader, string columnName)
        {
            var value = reader[columnName];
            return value == DBNull.Value ? (int?)null : Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }

        private static SqliteConnection CreateConnection(string filePath)
        {
            var connectionString = $"Data Source={filePath};";
            Log.Logger.Debug("SQL create connection: {connection}", connectionString);

            var connection = new SqliteConnection(connectionString);
            connection.Open();
            return connection;
        }

        private static void ClearData(SqliteConnection connection)
        {
            ClearTable(connection, "BlockRange");
            ClearTable(connection, "Bookmark");
            ClearTable(connection, "InputField");
            ClearTable(connection, "TagMap");
            ClearTable(connection, "Note");
            ClearTable(connection, "Tag");
            ClearTable(connection, "UserMark");
            ClearTable(connection, "Location");
            
            UpdateLastModified(connection);

            VacuumDatabase(connection);
        }

        private static void VacuumDatabase(SqliteConnection connection)
        {
            using var command = connection.CreateCommand();

            command.CommandText = "vacuum;";
            Log.Logger.Debug($"SQL: {command.CommandText}");

            command.ExecuteNonQuery();
        }

        private static void UpdateLastModified(SqliteConnection connection)
        {
            using var command = connection.CreateCommand();

            command.CommandText = "delete from LastModified; insert into LastModified default values";
            Log.Logger.Debug($"SQL: {command.CommandText}");

            command.ExecuteNonQuery();
        }

        private static void ClearTable(SqliteConnection connection, string tableName)
        {
            using var command = connection.CreateCommand();

            command.CommandText = $"delete from {tableName}";
            Log.Logger.Debug($"SQL: {command.CommandText}");

            command.ExecuteNonQuery();
        }

        private static void PopulateTable<TRowType>(SqliteConnection connection, List<TRowType> rows)
        {
            var tableName = typeof(TRowType).Name;
            var columnNames = GetColumnNames<TRowType>();
            var columnNamesCsv = string.Join(",", columnNames);
            var paramNames = GetParamNames(columnNames);
            var paramNamesCsv = string.Join(",", paramNames);

            using var transaction = connection.BeginTransaction();

            foreach (var row in rows)
            {
                if (row == null)
                {
                    continue;
                }

                using var cmd = connection.CreateCommand();
                cmd.CommandText = $"insert into {tableName} ({columnNamesCsv}) values ({paramNamesCsv})";
                AddPopulateTableParams(cmd, columnNames, paramNames, row);

                cmd.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        private static void AddPopulateTableParams<TRowType>(
            SqliteCommand cmd,
            List<string> columnNames,
            List<string> paramNames,
            TRowType row)
        {
            for (int n = 0; n < columnNames.Count; ++n)
            {
                var value = row!.GetType().GetProperty(columnNames[n])?.GetValue(row) ?? DBNull.Value;
                cmd.Parameters.AddWithValue(paramNames[n], value);
            }
        }

        private static List<string> GetParamNames(IReadOnlyCollection<string> columnNames)
        {
            return columnNames.Select(columnName => $"@{columnName}").ToList();
        }

        private static List<string> GetColumnNames<TRowType>()
        {
            var properties = typeof(TRowType).GetProperties();
            return properties.Select(property => property.Name).ToList();
        }

        private Location ReadLocation(SqliteDataReader reader)
        {
            return new Location
            {
                LocationId = ReadInt(reader, "LocationId"),
                BookNumber = ReadNullableInt(reader, "BookNumber"),
                ChapterNumber = ReadNullableInt(reader, "ChapterNumber"),
                DocumentId = ReadNullableInt(reader, "DocumentId"),
                Track = ReadNullableInt(reader, "Track"),
                IssueTagNumber = ReadInt(reader, "IssueTagNumber"),
                KeySymbol = ReadString(reader, "KeySymbol"),
                MepsLanguage = ReadInt(reader, "MepsLanguage"),
                Type = ReadInt(reader, "Type"),
                Title = ReadNullableString(reader, "Title"),
            };
        }

        private Note ReadNote(SqliteDataReader reader)
        {
            return new Note
            {
                NoteId = ReadInt(reader, "NoteId"),
                Guid = ReadString(reader, "Guid"),
                UserMarkId = ReadNullableInt(reader, "UserMarkId"),
                LocationId = ReadNullableInt(reader, "LocationId"),
                Title = ReadNullableString(reader, "Title"),
                Content = ReadNullableString(reader, "Content"),
                LastModified = ReadString(reader, "LastModified"),
                BlockType = ReadInt(reader, "BlockType"),
                BlockIdentifier = ReadNullableInt(reader, "BlockIdentifier"),
            };
        }

        private Tag ReadTag(SqliteDataReader reader)
        {
            return new Tag
            {
                TagId = ReadInt(reader, "TagId"),
                Type = ReadInt(reader, "Type"),
                Name = ReadString(reader, "Name"),
                ImageFileName = ReadNullableString(reader, "ImageFilename"),    // added in db v7 April 2020
            };
        }

        private TagMap ReadTagMap(SqliteDataReader reader)
        {
            return new TagMap
            {
                TagMapId = ReadInt(reader, "TagMapId"),
                
                // added in db v7, April 2020...
                PlaylistItemId = ReadNullableInt(reader, "PlaylistItemId"),
                LocationId = ReadNullableInt(reader, "LocationId"),
                NoteId = ReadNullableInt(reader, "NoteId"),

                TagId = ReadInt(reader, "TagId"),
                Position = ReadInt(reader, "Position"),
            };
        }

        private BlockRange ReadBlockRange(SqliteDataReader reader)
        {
            return new BlockRange
            {
                BlockRangeId = ReadInt(reader, "BlockRangeId"),
                BlockType = ReadInt(reader, "BlockType"),
                Identifier = ReadInt(reader, "Identifier"),
                StartToken = ReadNullableInt(reader, "StartToken"),
                EndToken = ReadNullableInt(reader, "EndToken"),
                UserMarkId = ReadInt(reader, "UserMarkId"),
            };
        }

        private Bookmark ReadBookmark(SqliteDataReader reader)
        {
            return new Bookmark
            {
                BookmarkId = ReadInt(reader, "BookmarkId"),
                LocationId = ReadInt(reader, "LocationId"),
                PublicationLocationId = ReadInt(reader, "PublicationLocationId"),
                Slot = ReadInt(reader, "Slot"),
                Title = ReadString(reader, "Title"),
                Snippet = ReadNullableString(reader, "Snippet"),
                BlockType = ReadInt(reader, "BlockType"),
                BlockIdentifier = ReadNullableInt(reader, "BlockIdentifier"),
            };
        }

        private LastModified ReadLastModified(SqliteDataReader reader)
        {
            return new LastModified
            {
                TimeLastModified = ReadString(reader, "LastModified"),
            };
        }
        
        private UserMark ReadUserMark(SqliteDataReader reader)
        {
            return new UserMark
            {
                UserMarkId = ReadInt(reader, "UserMarkId"),
                ColorIndex = ReadInt(reader, "ColorIndex"),
                LocationId = ReadInt(reader, "LocationId"),
                StyleIndex = ReadInt(reader, "StyleIndex"),
                UserMarkGuid = ReadString(reader, "UserMarkGuid"),
                Version = ReadInt(reader, "Version"),
            };
        }

        private InputField ReadInputField(SqliteDataReader reader)
        {
            return new InputField
            {
                LocationId = ReadInt(reader, "LocationId"),
                TextTag = ReadString(reader, "TextTag"),
                Value = ReadString(reader, "Value"),
            };
        }

        private SqliteConnection CreateConnection()
        {
            return CreateConnection(_databaseFilePath);
        }
    }
}
