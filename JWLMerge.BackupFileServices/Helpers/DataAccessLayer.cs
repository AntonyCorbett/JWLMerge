namespace JWLMerge.BackupFileServices.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using JWLMerge.BackupFileServices.Models.DatabaseModels;
    using Serilog;

    /// <summary>
    /// Isolates all data access to the SQLite database embedded in
    /// jwlibrary files.
    /// </summary>
    internal class DataAccessLayer
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
            
            using (var source = CreateConnection(_databaseFilePath))
            using (var destination = CreateConnection(cloneFilePath))
            {
                source.BackupDatabase(destination, "main", "main", -1, null, -1);
                ClearData(destination);
            }
        }

        /// <summary>
        /// Populates the current database using the specified data.
        /// </summary>
        /// <param name="dataToUse">The data to use.</param>
        public void PopulateTables(Database dataToUse)
        {
            using (var connection = CreateConnection())
            {
                PopulateTable(connection, dataToUse.Locations);
                PopulateTable(connection, dataToUse.Notes);
                PopulateTable(connection, dataToUse.UserMarks);
                PopulateTable(connection, dataToUse.Tags);
                PopulateTable(connection, dataToUse.TagMaps);
                PopulateTable(connection, dataToUse.BlockRanges);
                PopulateTable(connection, dataToUse.Bookmarks);
            }
        }

        /// <summary>
        /// Reads the current database.
        /// </summary>
        /// <returns><see cref="Database"/></returns>
        public Database ReadDatabase()
        {
            var result = new Database();

            using (var connection = CreateConnection())
            {
                result.InitBlank();

                result.LastModified.TimeLastModified = ReadAllRows(connection, ReadLastModified)?.FirstOrDefault()?.TimeLastModified;
                result.Locations.AddRange(ReadAllRows(connection, ReadLocation));
                result.Notes.AddRange(ReadAllRows(connection, ReadNote));
                result.Tags.AddRange(ReadAllRows(connection, ReadTag));
                result.TagMaps.AddRange(ReadAllRows(connection, ReadTagMap));
                result.BlockRanges.AddRange(ReadAllRows(connection, ReadBlockRange));
                result.Bookmarks.AddRange(ReadAllRows(connection, ReadBookmark));
                result.UserMarks.AddRange(ReadAllRows(connection, ReadUserMark));

                // ensure bookmarks appear in similar order to original.
                result.Bookmarks.Sort((bookmark1, bookmark2) => bookmark1.Slot.CompareTo(bookmark2.Slot));
            }

            return result;
        }

        private List<TRowType> ReadAllRows<TRowType>(
            SQLiteConnection connection,
            Func<SQLiteDataReader, TRowType> readRowFunction)
        {
            using (SQLiteCommand cmd = connection.CreateCommand())
            {
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

                Log.Logger.Debug($"SQL resultset count: {result.Count}");
                
                return result;
            }
        }

        private string ReadString(SQLiteDataReader reader, string columnName)
        {
            return reader[columnName].ToString();
        }

        private string ReadNullableString(SQLiteDataReader reader, string columnName)
        {
            var value = reader[columnName];
            return value == DBNull.Value ? null : value.ToString();
        }

        private int ReadInt(SQLiteDataReader reader, string columnName)
        {
            return Convert.ToInt32(reader[columnName]);
        }

        private int? ReadNullableInt(SQLiteDataReader reader, string columnName)
        {
            var value = reader[columnName];
            return value == DBNull.Value ? (int?)null : Convert.ToInt32(value);
        }

        private Location ReadLocation(SQLiteDataReader reader)
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

        private Note ReadNote(SQLiteDataReader reader)
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

        private Tag ReadTag(SQLiteDataReader reader)
        {
            return new Tag
            {
                TagId = ReadInt(reader, "TagId"),
                Type = ReadInt(reader, "Type"),
                Name = ReadString(reader, "Name"),
                ImageFileName = ReadNullableString(reader, "ImageFilename"),    // added in db v7 April 2020
            };
        }

        private TagMap ReadTagMap(SQLiteDataReader reader)
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

        private BlockRange ReadBlockRange(SQLiteDataReader reader)
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

        private Bookmark ReadBookmark(SQLiteDataReader reader)
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

        private LastModified ReadLastModified(SQLiteDataReader reader)
        {
            return new LastModified
            {
                TimeLastModified = ReadString(reader, "LastModified"),
            };
        }
        
        private UserMark ReadUserMark(SQLiteDataReader reader)
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

        private SQLiteConnection CreateConnection()
        {
            return CreateConnection(_databaseFilePath);
        }
        
        private SQLiteConnection CreateConnection(string filePath)
        {
            var connectionString = $"Data Source={filePath};Version=3;";
            Log.Logger.Debug("SQL create connection: {connection}", connectionString);
            
            var connection = new SQLiteConnection(connectionString);
            connection.Open();
            return connection;
        }

        private void ClearData(SQLiteConnection connection)
        {
            ClearTable(connection, "UserMark");
            ClearTable(connection, "TagMap");
            ClearTable(connection, "Tag");
            ClearTable(connection, "Note");
            ClearTable(connection, "Location");
            ClearTable(connection, "Bookmark");
            ClearTable(connection, "BlockRange");

            UpdateLastModified(connection);

            VacuumDatabase(connection);
        }

        private void VacuumDatabase(SQLiteConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "vacuum;";
                Log.Logger.Debug($"SQL: {command.CommandText}");
                
                command.ExecuteNonQuery();
            }
        }

        private void UpdateLastModified(SQLiteConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "delete from LastModified; insert into LastModified default values";
                Log.Logger.Debug($"SQL: {command.CommandText}");
                
                command.ExecuteNonQuery();
            }
        }

        private void ClearTable(SQLiteConnection connection, string tableName)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = $"delete from {tableName}";
                Log.Logger.Debug($"SQL: {command.CommandText}");
                
                command.ExecuteNonQuery();
            }
        }

        private void PopulateTable<TRowType>(SQLiteConnection connection, List<TRowType> rows)
        {
            var tableName = typeof(TRowType).Name;
            var columnNames = GetColumnNames<TRowType>();
            var columnNamesCsv = string.Join(",", columnNames);
            var paramNames = GetParamNames(columnNames);
            var paramNamesCsv = string.Join(",", paramNames);

            using (var transaction = connection.BeginTransaction())
            {
                foreach (var row in rows)
                {
                    using (SQLiteCommand cmd = connection.CreateCommand())
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine($"insert into {tableName} ({columnNamesCsv}) values ({paramNamesCsv})");

                        cmd.CommandText = sb.ToString();
                        AddPopulateTableParams(cmd, columnNames, paramNames, row);

                        cmd.ExecuteNonQuery();
                    }
                }
                
                transaction.Commit();
            }
        }

        private void AddPopulateTableParams<TRowType>(
            SQLiteCommand cmd, 
            List<string> columnNames,
            List<string> paramNames, 
            TRowType row)
        {
            for (int n = 0; n < columnNames.Count; ++n)
            {
                var value = row.GetType().GetProperty(columnNames[n])?.GetValue(row);
                cmd.Parameters.AddWithValue(paramNames[n], value);
            }
        }

        private List<string> GetParamNames(IReadOnlyCollection<string> columnNames)
        {
            return columnNames.Select(columnName => $"@{columnName}").ToList();
        }

        private List<string> GetColumnNames<TRowType>()
        {
            PropertyInfo[] properties = typeof(TRowType).GetProperties();
            return properties.Select(property => property.Name).ToList();
        }
    }
}
