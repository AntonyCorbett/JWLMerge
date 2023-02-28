using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using JWLMerge.ExcelServices.Exceptions;
using JWLMerge.ImportExportServices;
using JWLMerge.ImportExportServices.Models;

namespace JWLMerge.ExcelServices;

public class ExcelService : IExportToFileService
{
    private const string WorkbookName = "Notes";

    /// <summary>
    /// Exports Bible notes to spreadsheet page
    /// </summary>
    /// <param name="exportFilePath">Excel file path.</param>
    /// <param name="notes">Notes.</param>
    /// <param name="backupFilePath">Path to backup file.</param>
    /// <returns>Results.</returns>
    public ExportBibleNotesResult Execute(
        string exportFilePath, 
        IReadOnlyCollection<BibleNoteForImportExport>? notes,
        string backupFilePath)
    {
        var result = new ExportBibleNotesResult();
        var startRow = 0;

        if (string.IsNullOrEmpty(exportFilePath))
        {
            throw new ArgumentNullException(nameof(exportFilePath));
        }

        if (!File.Exists(exportFilePath))
        {
            var lastRow = GenerateHeader(exportFilePath, backupFilePath);
            startRow = lastRow + 2;
        }

        if (notes == null || !notes.Any())
        {
            result.NoNotesFound = true;
            return result;
        }

        using var workbook = new XLWorkbook(exportFilePath);

        if (!workbook.Worksheets.TryGetWorksheet(WorkbookName, out var worksheet))
        {
            throw new ExcelServicesException("Could not find worksheet!");
        }

        var row = startRow;
        foreach (var note in notes)
        {
            var noteTooLarge = note.NoteContent != null && note.NoteContent.Length > Int16.MaxValue;
            if (noteTooLarge)
            {
                result.SomeNotesTooLarge = true;
            }
                
            var noteContent = noteTooLarge
                ? note.NoteContent![..short.MaxValue]
                : note.NoteContent;

            SetCellStringValue(worksheet, row, 1, note.PubSymbol);
            SetCellIntegerValue(worksheet, row, 2, note.MepsLanguageId);
            SetCellStringValue(worksheet, row, 3, note.BookName);
            SetCellIntegerValue(worksheet, row, 4, note.BookNumber);
            SetCellIntegerValue(worksheet, row, 5, note.ChapterNumber);
            SetCellIntegerValue(worksheet, row, 6, note.VerseNumber);
            SetCellStringValue(worksheet, row, 7, note.ChapterAndVerseString);
            SetCellStringValue(worksheet, row, 8, note.BookNameChapterAndVerseString);
            SetCellIntegerValue(worksheet, row, 9, note.ColorCode);
            SetCellStringValue(worksheet, row, 10, note.TagsCsv);
            SetCellIntegerValue(worksheet, row, 11, note.StartTokenInVerse);
            SetCellIntegerValue(worksheet, row, 12, note.EndTokenInVerse);
            SetCellStringValue(worksheet, row, 13, note.NoteTitle);
            SetCellStringValue(worksheet, row, 14, noteContent);

            ++row;
        }

        workbook.Save();

        result.RowCount = row;
        return result;
    }

    private static void SetCellStringValue(IXLWorksheet worksheet, int row, int col, string? value)
    {
        worksheet.Cell(row, col).SetValue(value ?? string.Empty);
    }

    private static void SetCellIntegerValue(IXLWorksheet worksheet, int row, int col, int? value)
    {
        worksheet.Cell(row, col).SetValue(value);
    }

    private static int GenerateHeader(string excelFilePath, string backupFilePath)
    {
        using var workbook = new XLWorkbook();

        var worksheet = workbook.Worksheets.Add(WorkbookName);
        worksheet.Cell("A1").Value = "Bible Notes Extracted from JWL Backup File";
        worksheet.Cell("A2").Value = $"Backup file: {backupFilePath}";
        worksheet.Cell("A3").Value = $"Exported: {DateTime.Now.ToShortDateString()}";

        worksheet.Cell("A5").Value = "Symbol";
        worksheet.Cell("B5").Value = "LanguageId";
        worksheet.Cell("C5").Value = "Book";
        worksheet.Cell("D5").Value = "BookNumber";
        worksheet.Cell("E5").Value = "Chapter";
        worksheet.Cell("F5").Value = "Verse";
        worksheet.Cell("G5").Value = "ChapterAndVerse";
        worksheet.Cell("H5").Value = "FullRef";
        worksheet.Cell("I5").Value = "Color";
        worksheet.Cell("J5").Value = "Tags";
        worksheet.Cell("K5").Value = "StartToken";
        worksheet.Cell("L5").Value = "EndToken";
        worksheet.Cell("M5").Value = "Title";
        worksheet.Cell("N5").Value = "Content";

        workbook.SaveAs(excelFilePath);

        // return last row used.
        return 4;
    }
}