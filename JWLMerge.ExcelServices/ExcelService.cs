using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using JWLMerge.ExcelServices.Exceptions;
using JWLMerge.ExcelServices.Models;

namespace JWLMerge.ExcelServices
{
    public class ExcelService : IExcelService
    {
        private const string WorkbookName = "Notes";

        // returns the last row written
        public int AppendToBibleNotesFile(
            string excelFilePath, 
            IReadOnlyCollection<BibleNote>? notes, 
            int startRow, 
            string backupFilePath)
        {
            if (string.IsNullOrEmpty(excelFilePath))
            {
                throw new ArgumentNullException(nameof(excelFilePath));
            }

            if (!File.Exists(excelFilePath))
            {
                var lastRow = GenerateHeader(excelFilePath, backupFilePath);
                startRow = lastRow + 2;
            }

            if (notes == null || !notes.Any())
            {
                return startRow;
            }

            using var workbook = new XLWorkbook(excelFilePath);

            if (!workbook.Worksheets.TryGetWorksheet(WorkbookName, out var worksheet))
            {
                throw new ExcelServicesException("Could not find worksheet!");
            }

            var row = startRow;
            foreach (var note in notes)
            {
                SetCellStringValue(worksheet, row, 1, note.PubSymbol);
                SetCellStringValue(worksheet, row, 2, note.BookName);
                SetCellIntegerValue(worksheet, row, 3, note.BookNumber);
                SetCellIntegerValue(worksheet, row, 4, note.ChapterNumber);
                SetCellIntegerValue(worksheet, row, 5, note.VerseNumber);
                SetCellStringValue(worksheet, row, 6, note.ChapterAndVerseString);
                SetCellStringValue(worksheet, row, 7, note.BookNameChapterAndVerseString);
                SetCellIntegerValue(worksheet, row, 8, note.ColorCode);
                SetCellStringValue(worksheet, row, 9, note.TagsCsv);
                SetCellStringValue(worksheet, row, 10, note.NoteTitle);
                SetCellStringValue(worksheet, row, 11, note.NoteContent);

                ++row;
            }

            workbook.Save();

            return row;
        }

        private static void SetCellStringValue(IXLWorksheet worksheet, int row, int col, string? value)
        {
            worksheet.Cell(row, col).DataType = XLDataType.Text;
            worksheet.Cell(row, col).SetValue(value ?? string.Empty);
        }

        private static void SetCellIntegerValue(IXLWorksheet worksheet, int row, int col, int? value)
        {
            worksheet.Cell(row, col).DataType = XLDataType.Number;
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
            worksheet.Cell("B5").Value = "Book";
            worksheet.Cell("C5").Value = "BookNumber";
            worksheet.Cell("D5").Value = "Chapter";
            worksheet.Cell("E5").Value = "Verse";
            worksheet.Cell("F5").Value = "ChapterAndVerse";
            worksheet.Cell("G5").Value = "FullRef";
            worksheet.Cell("H5").Value = "Color";
            worksheet.Cell("I5").Value = "Tags";
            worksheet.Cell("J5").Value = "Title";
            worksheet.Cell("K5").Value = "Content";

            workbook.SaveAs(excelFilePath);

            // return last row used.
            return 4;
        }
    }
}
