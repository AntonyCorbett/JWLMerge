using System.Globalization;
using System;
using System.Collections.Generic;
using JWLMerge.BackupFileServices.Models;
using JWLMerge.BackupFileServices.Models.DatabaseModels;
using JWLMerge.BackupFileServices.Models.ManifestFile;

namespace JWLMerge.Tests;

public class TestBase
{
    private readonly Random _random = new();
        
    protected BackupFile CreateMockBackup(int numRecords = 100)
    {
        return new(CreateMockManifest(), CreateMockDatabase(numRecords), "test.jwlibrary");
    }

    private Database CreateMockDatabase(int numRecords)
    {
        var result = new Database();

        result.InitBlank();

        result.LastModified.TimeLastModified = "2018-01-20T11:35:00Z";
        result.UserMarks.AddRange(CreateMockUserMarks(numRecords));
        result.Locations.AddRange(CreateMockLocations(numRecords));
        result.Notes.AddRange(CreateMockNotes(numRecords));
        result.BlockRanges.AddRange(CreateMockBlockRanges(numRecords));

        return result;
    }

    private static Manifest CreateMockManifest()
    {
        DateTime now = DateTime.Now;
        string simpleDateString = $"{now.Year}-{now.Month:D2}-{now.Day:D2}";

        return new Manifest
        {
            Name = "Test",
            CreationDate = simpleDateString,
            Version = 1,
            UserDataBackup = new UserDataBackup
            {
                DatabaseName = "userData.db",
                Hash = "123",
                DeviceName = "Test",
                LastModifiedDate = simpleDateString,
                SchemaVersion = 5,
            },
        };
    }

    private static List<BlockRange> CreateMockBlockRanges(int numRecords)
    {
        var result = new List<BlockRange>();

        for (int n = 1; n <= numRecords; ++n)
        {
            result.Add(new BlockRange
            {
                BlockRangeId = n,
                UserMarkId = n,
                BlockType = 2,
                EndToken = 4,
                Identifier = 8,
            });
        }
            
        return result;
    }

    private List<Note> CreateMockNotes(int numRecords)
    {
        var result = new List<Note>();
            
        for (int n = 1; n <= numRecords; ++n)
        {
            result.Add(new Note
            {
                NoteId = n,
                Guid = Guid.NewGuid().ToString().ToLower(CultureInfo.InvariantCulture),
                UserMarkId = n,
                LocationId = n,
                Title = $"Title {n}",
                Content = $"Content {n}",
                LastModified = "2018-01-20T11:35:00Z",
                BlockType = 2,
                BlockIdentifier = _random.Next(1, 10),
            });
        }

        return result;
    }

    private static List<UserMark> CreateMockUserMarks(int numRecords)
    {
        var result = new List<UserMark>();

        for (int n = 1; n <= numRecords; ++n)
        {
            result.Add(new UserMark
            {
                UserMarkId = n,
                ColorIndex = 1,
                LocationId = n,
                UserMarkGuid = Guid.NewGuid().ToString().ToLower(CultureInfo.InvariantCulture),
                Version = 1,
            });
        }

        return result;
    }

    private List<Location> CreateMockLocations(int numRecords)
    {
        var result = new List<Location>();

        for (int n = 1; n <= numRecords; ++n)
        {
            result.Add(new Location
            {
                LocationId = n,
                Title = $"Title {n}",
                BookNumber = _random.Next(1, 67),
                ChapterNumber = _random.Next(1, 30),
                KeySymbol = "nwtsty",
            });
        }

        return result;
    }
}