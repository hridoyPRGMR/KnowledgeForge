using KnowledgeForge.Application.Configuration;
using KnowledgeForge.Application.DTOs;
using KnowledgeForge.Application.Interfaces;
using KnowledgeForge.Domain.Entities;
using KnowledgeForge.Domain.Enums;
using KnowledgeForge.Domain.Events;
using KnowledgeForge.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace KnowledgeForge.Infrastructure.Services;

public class BookService(
    AppDbContext db,
    IBus bus,
    IStorageService storageService) : IBookService
{

    public async Task<BookDto> UploadBookAsync(Stream fileStream, string fileName, CancellationToken ct = default)
    {
        // 1. Generate unique identifiers and safe names
        var bookId = Guid.NewGuid();
        var safeFileName = Path.GetFileName(fileName);
        var title = Path.GetFileNameWithoutExtension(safeFileName);
        
        // Define a universal storage path identifier (e.g., "books/guid.pdf")
        var storagePath = $"{bookId}{Path.GetExtension(safeFileName)}";

        // 2. Delegate the upload to the interface
        // This will save to local disk OR Azure Blob depending on your settings!
        await storageService.UploadFileAsync(storagePath, fileStream, ct);

        // 3. Save metadata to the database
        var book = new Book
        {
            Id = bookId,
            Title = title,
            FilePath = storagePath, // Save the logical path reference
            Status = BookStatus.Uploaded
        };

        db.Books.Add(book);
        await db.SaveChangesAsync(ct);
        
        // 4. Publish event for any background processing
        await bus.Publish(new BookUploadedEvent(bookId, storagePath), ct);

        return new BookDto(book.Id, book.Title, book.Status, book.PageCount, book.CreatedAt, 0);
    }

    public async Task<IReadOnlyList<BookDto>> GetBooksAsync(CancellationToken ct = default)
    {
        return await db.Books
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new BookDto(b.Id, b.Title, b.Status, b.PageCount, b.CreatedAt, b.Chapters.Count))
            .ToListAsync(ct);
    }

    public async Task<BookDetailDto?> GetBookAsync(Guid id, CancellationToken ct = default)
    {
        var book = await db.Books
            .Include(b => b.Chapters.OrderBy(c => c.ChapterNumber))
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (book is null)
        {
            return null;
        }

        var chapters = book.Chapters
            .Select(c => new ChapterDto(c.Id, c.ChapterNumber, c.Title, c.StartPage, c.EndPage))
            .ToList();

        return new BookDetailDto(book.Id, book.Title, book.Status, book.PageCount, book.ErrorMessage, book.CreatedAt, chapters);
    }

    public async Task<BookStatusDto?> GetBookStatusAsync(Guid id, CancellationToken ct = default)
    {
        var book = await db.Books
            .Where(b => b.Id == id)
            .Select(b => new { b.Id, b.Status, b.ErrorMessage, ChunkCount = b.Chunks.Count })
            .FirstOrDefaultAsync(ct);

        return book is null ? null : new BookStatusDto(book.Id, book.Status, book.ErrorMessage, book.ChunkCount);
    }

    public async Task<ChapterContentDto?> GetChapterContentAsync(Guid bookId, Guid chapterId, CancellationToken ct = default)
    {
        var chapter = await db.BookChapters
            .Where(c => c.BookId == bookId && c.Id == chapterId)
            .Select(c => new { c.Id, c.Title, c.ChapterNumber, Content = string.Join("\n\n", c.Chunks.OrderBy(x => x.ChunkIndex).Select(x => x.Content)) })
            .FirstOrDefaultAsync(ct);

        return chapter is null ? null : new ChapterContentDto(chapter.Id, chapter.Title, chapter.ChapterNumber, chapter.Content);
    }
}
