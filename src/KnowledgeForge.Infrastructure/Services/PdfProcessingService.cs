using System.Text;
using System.Text.RegularExpressions;
using KnowledgeForge.Application.Interfaces;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace KnowledgeForge.Infrastructure.Services;

public partial class PdfProcessingService : IPdfProcessingService
{
    public Task<PdfExtractionResult> ExtractAsync(string filePath, int chunkSize, int chunkOverlap, CancellationToken ct = default)
    {
        using var document = PdfDocument.Open(filePath);
        var pageCount = document.NumberOfPages;
        var pages = new List<(int PageNumber, string Text)>();

        foreach (var page in document.GetPages())
        {
            var text = page.Text?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(text))
            {
                pages.Add((page.Number, text));
            }
        }

        var chapters = DetectChapters(pages, chunkSize, chunkOverlap);
        return Task.FromResult(new PdfExtractionResult(pageCount, chapters));
    }

    private static IReadOnlyList<PdfChapterResult> DetectChapters(
        IReadOnlyList<(int PageNumber, string Text)> pages,
        int chunkSize,
        int chunkOverlap)
    {
        if (pages.Count == 0)
        {
            return [];
        }

        var chapterStarts = new List<(int PageNumber, string Title)>();
        for (var i = 0; i < pages.Count; i++)
        {
            var (pageNumber, text) = pages[i];
            var firstLine = text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault() ?? string.Empty;

            if (ChapterHeadingRegex().IsMatch(firstLine) || ChapterNumberRegex().IsMatch(firstLine))
            {
                chapterStarts.Add((pageNumber, firstLine.Length > 100 ? firstLine[..100] : firstLine));
            }
        }

        if (chapterStarts.Count == 0)
        {
            var fullText = string.Join("\n\n", pages.Select(p => p.Text));
            return
            [
                new PdfChapterResult(1, "Full Book", pages.First().PageNumber, pages.Last().PageNumber, fullText, ChunkText(fullText, chunkSize, chunkOverlap))
            ];
        }

        var results = new List<PdfChapterResult>();
        for (var i = 0; i < chapterStarts.Count; i++)
        {
            var startPage = chapterStarts[i].PageNumber;
            var endPage = i + 1 < chapterStarts.Count ? chapterStarts[i + 1].PageNumber - 1 : pages.Last().PageNumber;
            var chapterPages = pages.Where(p => p.PageNumber >= startPage && p.PageNumber <= endPage).ToList();
            var content = string.Join("\n\n", chapterPages.Select(p => p.Text));
            results.Add(new PdfChapterResult(
                i + 1,
                chapterStarts[i].Title,
                startPage,
                endPage,
                content,
                ChunkText(content, chunkSize, chunkOverlap)));
        }

        return results;
    }

    private static IReadOnlyList<PdfChunkResult> ChunkText(string text, int chunkSize, int chunkOverlap)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
        {
            return [];
        }

        var chunks = new List<PdfChunkResult>();
        var index = 0;
        var chunkIndex = 0;

        while (index < words.Length)
        {
            var end = Math.Min(index + chunkSize, words.Length);
            var chunkWords = words[index..end];
            var content = string.Join(' ', chunkWords);
            chunks.Add(new PdfChunkResult(chunkIndex++, content, chunkWords.Length));

            if (end >= words.Length)
            {
                break;
            }

            index = Math.Max(end - chunkOverlap, index + 1);
        }

        return chunks;
    }

    [GeneratedRegex(@"^Chapter\s+\d+", RegexOptions.IgnoreCase)]
    private static partial Regex ChapterHeadingRegex();

    [GeneratedRegex(@"^(CHAPTER|Chapter)\s+(\d+|[IVXLCDM]+)", RegexOptions.IgnoreCase)]
    private static partial Regex ChapterNumberRegex();
}
