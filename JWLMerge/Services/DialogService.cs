using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JWLMerge.BackupFileServices.Models;
using JWLMerge.BackupFileServices.Models.DatabaseModels;
using JWLMerge.BackupFileServices.Exceptions;
using JWLMerge.Dialogs;
using MaterialDesignThemes.Wpf;
using JWLMerge.EventTracking;
using JWLMerge.Models;
using JWLMerge.ViewModel;

namespace JWLMerge.Services;

#pragma warning disable CA1416 // Validate platform compatibility

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class DialogService : IDialogService
{
    public const int UntaggedItemId = -1;
    private const string NotTaggedString = "** Not Tagged **";

    private bool _isDialogVisible;

    public async Task ShowFileFormatErrorsAsync(AggregateException ex)
    {
        _isDialogVisible = true;

        var dialog = new FileFormatErrorDialog();
        var dc = (BackupFileFormatErrorViewModel)dialog.DataContext;

        dc.Errors.Clear();

        foreach (var e in ex.InnerExceptions)
        {
            if (e is BackupFileServicesException bex)
            {
                switch (e)
                {
                    case WrongDatabaseVersionException dbVerEx:
                        EventTracker.TrackWrongVer(dbVerEx.FoundVersion, dbVerEx.ExpectedVersion);
                        dc.Errors.Add(new FileFormatErrorListItem(dbVerEx.Filename ?? "Error", dbVerEx.Message));
                        break;
                    case WrongManifestVersionException mftVerEx:
                        EventTracker.TrackWrongManifestVer(mftVerEx.FoundVersion, mftVerEx.ExpectedVersion);
                        dc.Errors.Add(new FileFormatErrorListItem(mftVerEx.Filename ?? "Error", mftVerEx.Message));
                        break;
                    default:
                        EventTracker.Error(bex, "Importing file");
                        dc.Errors.Add(new FileFormatErrorListItem("Error", bex.Message));
                        break;
                }
            }
        }

        await DialogHost.Show(
            dialog,
            "MainDialogHost",
            (object _, DialogClosingEventArgs _) => _isDialogVisible = false).ConfigureAwait(false);
    }

    public async Task<bool> ShouldRemoveFavouritesAsync()
    {
        _isDialogVisible = true;

        var dialog = new RemoveFavouritesPromptDialog();
        var dc = (RemoveFavouritesPromptViewModel)dialog.DataContext;

        await DialogHost.Show(
            dialog,
            "MainDialogHost",
            (object _, DialogClosingEventArgs _) => _isDialogVisible = false).ConfigureAwait(false);

        return dc.Result;
    }

    public async Task<bool> ShouldRedactNotesAsync()
    {
        _isDialogVisible = true;

        var dialog = new RedactNotesPromptDialog();
        var dc = (RedactNotesPromptViewModel)dialog.DataContext;

        await DialogHost.Show(
            dialog, 
            "MainDialogHost",
            (object _, DialogClosingEventArgs _) => _isDialogVisible = false).ConfigureAwait(false);

        return dc.Result;
    }

    public async Task<NotesByTagResult> GetTagSelectionForNotesRemovalAsync(Tag[] tags, bool includeUntaggedNotes)
    {
        _isDialogVisible = true;

        var dialog = new RemoveNotesByTagDialog();
        var dc = (RemoveNotesByTagViewModel)dialog.DataContext;

        dc.RemoveAssociatedUnderlining = true;
        dc.RemoveAssociatedTags = false;
        dc.TagItems.Clear();

        if (includeUntaggedNotes)
        {
            dc.TagItems.Add(new TagListItem(NotTaggedString, UntaggedItemId));
        }

        foreach (var tag in tags)
        {
            dc.TagItems.Add(new TagListItem(tag.Name, tag.TagId));
        }

        await DialogHost.Show(
            dialog,
            "MainDialogHost",
            (object _, DialogClosingEventArgs _) => _isDialogVisible = false).ConfigureAwait(false);

        var removeUntaggedNotes = dc.Result != null && dc.Result.Contains(UntaggedItemId);
        var tagsIdsResult = dc.Result?.Where(x => x != UntaggedItemId).ToArray();

        return new NotesByTagResult
        {
            TagIds = tagsIdsResult,
            RemoveUntaggedNotes = removeUntaggedNotes,
            RemoveAssociatedUnderlining = dc.RemoveAssociatedUnderlining,
            RemoveAssociatedTags = dc.RemoveAssociatedTags,
        };
    }

    public async Task<ColorResult> GetColourSelectionForUnderlineRemovalAsync(ColourDef[] colours)
    {
        _isDialogVisible = true;

        var dialog = new RemoveUnderliningByColourDialog();
        var dc = (RemoveUnderliningByColourViewModel)dialog.DataContext;

        dc.RemoveAssociatedNotes = true;
        dc.ColourItems.Clear();

        foreach (var c in colours)
        {
            dc.ColourItems.Add(new ColourListItem(c.Name, c.ColourIndex, c.Color));
        }

        await DialogHost.Show(
            dialog,
            "MainDialogHost",
            (object _, DialogClosingEventArgs _) => _isDialogVisible = false).ConfigureAwait(false);

        return new ColorResult
        {
            ColourIndexes = dc.Result,
            RemoveNotes = dc.RemoveAssociatedNotes,
        };
    }

    public async Task<PubColourResult?> GetPubAndColourSelectionForUnderlineRemovalAsync(PublicationDef[] pubs, ColourDef[] colors)
    {
        _isDialogVisible = true;

        var dialog = new RemoveUnderliningByPubAndColourDialog();
        var dc = (RemoveUnderliningByPubAndColourViewModel)dialog.DataContext;

        dc.RemoveAssociatedNotes = true;
        dc.ColourItems.Clear();
        dc.PublicationList.Clear();

        foreach (var c in colors)
        {
            dc.ColourItems.Add(new ColourListItem(c.Name, c.ColourIndex, c.Color));
        }

        foreach (var p in pubs)
        {
            dc.PublicationList.Add(p);
        }

        await DialogHost.Show(
            dialog,
            "MainDialogHost",
            (object _, DialogClosingEventArgs _) => _isDialogVisible = false).ConfigureAwait(false);

        return dc.Result;
    }

    public async Task<ImportBibleNotesParams?> GetImportBibleNotesParamsAsync(IReadOnlyCollection<Tag> databaseTags)
    {
        _isDialogVisible = true;

        var dialog = new ImportBibleNotesDialog();
        var dc = (ImportBibleNotesViewModel)dialog.DataContext;

        dc.Tags = databaseTags;
        dc.SelectedTagId = 0;

        await DialogHost.Show(
            dialog,
            "MainDialogHost",
            (object _, DialogClosingEventArgs _) => _isDialogVisible = false).ConfigureAwait(false);

        return dc.Result;
    }

    public bool IsDialogVisible()
    {
        return _isDialogVisible;
    }
}

#pragma warning restore CA1416 // Validate platform compatibility