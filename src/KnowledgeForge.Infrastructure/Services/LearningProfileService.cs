using KnowledgeForge.Application.DTOs;
using KnowledgeForge.Application.Interfaces;
using KnowledgeForge.Domain.Entities;
using KnowledgeForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeForge.Infrastructure.Services;

public class LearningProfileService(AppDbContext db) : ILearningProfileService
{
    public async Task<ReadingProgressDto?> GetProgressAsync(Guid bookId, CancellationToken ct = default)
    {
        var progress = await db.ReadingProgress
            .Include(p => p.Book)
            .FirstOrDefaultAsync(p => p.BookId == bookId, ct);

        return progress is null ? null : Map(progress);
    }

    public async Task<ReadingProgressDto> UpdateProgressAsync(Guid bookId, UpdateProgressDto dto, CancellationToken ct = default)
    {
        var book = await db.Books.FirstOrDefaultAsync(b => b.Id == bookId, ct)
            ?? throw new KeyNotFoundException("Book not found.");

        var progress = await db.ReadingProgress.FirstOrDefaultAsync(p => p.BookId == bookId, ct);
        if (progress is null)
        {
            progress = new ReadingProgress { Id = Guid.NewGuid(), BookId = bookId };
            db.ReadingProgress.Add(progress);
        }

        progress.CurrentChapter = dto.CurrentChapter;
        progress.CurrentPage = dto.CurrentPage;
        progress.PercentComplete = dto.PercentComplete;
        progress.LastReadAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        progress.Book = book;
        return Map(progress);
    }

    public async Task<ProfileDto> GetProfileAsync(CancellationToken ct = default)
    {
        var books = await db.Books.ToListAsync(ct);
        var progressList = await db.ReadingProgress
            .Include(p => p.Book)
            .OrderByDescending(p => p.LastReadAt)
            .Take(5)
            .ToListAsync(ct);

        var readyBooks = books.Count(b => b.Status == Domain.Enums.BookStatus.Ready);
        var avgProgress = progressList.Count > 0 ? progressList.Average(p => p.PercentComplete) : 0;
        var chaptersRead = progressList.Sum(p => p.CurrentChapter);

        return new ProfileDto(
            books.Count,
            readyBooks,
            chaptersRead,
            avgProgress,
            progressList.Select(Map).ToList());
    }

    private static ReadingProgressDto Map(ReadingProgress p) =>
        new(p.BookId, p.Book.Title, p.CurrentChapter, p.CurrentPage, p.PercentComplete, p.LastReadAt);
}
