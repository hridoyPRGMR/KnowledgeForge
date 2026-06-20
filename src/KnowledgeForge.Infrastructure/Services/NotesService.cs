using KnowledgeForge.Application.DTOs;
using KnowledgeForge.Application.Interfaces;
using KnowledgeForge.Domain.Entities;
using KnowledgeForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeForge.Infrastructure.Services;

public class NotesService(AppDbContext db) : INotesService
{
    public async Task<NoteDto> CreateNoteAsync(Guid bookId, CreateNoteDto dto, CancellationToken ct = default)
    {
        _ = await db.Books.FirstOrDefaultAsync(b => b.Id == bookId, ct)
            ?? throw new KeyNotFoundException("Book not found.");

        var note = new Note
        {
            Id = Guid.NewGuid(),
            BookId = bookId,
            ChapterId = dto.ChapterId,
            Content = dto.Content
        };

        db.Notes.Add(note);
        await db.SaveChangesAsync(ct);
        return new NoteDto(note.Id, note.BookId, note.ChapterId, note.Content, note.CreatedAt);
    }

    public async Task<IReadOnlyList<NoteDto>> GetNotesAsync(Guid bookId, CancellationToken ct = default)
    {
        return await db.Notes
            .Where(n => n.BookId == bookId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NoteDto(n.Id, n.BookId, n.ChapterId, n.Content, n.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task DeleteNoteAsync(Guid id, CancellationToken ct = default)
    {
        var note = await db.Notes.FindAsync([id], ct);
        if (note is not null)
        {
            db.Notes.Remove(note);
            await db.SaveChangesAsync(ct);
        }
    }

    public async Task<HighlightDto> CreateHighlightAsync(Guid bookId, CreateHighlightDto dto, CancellationToken ct = default)
    {
        _ = await db.Books.FirstOrDefaultAsync(b => b.Id == bookId, ct)
            ?? throw new KeyNotFoundException("Book not found.");

        var highlight = new Highlight
        {
            Id = Guid.NewGuid(),
            BookId = bookId,
            ChapterId = dto.ChapterId,
            Text = dto.Text,
            StartOffset = dto.StartOffset,
            EndOffset = dto.EndOffset,
            Color = dto.Color
        };

        db.Highlights.Add(highlight);
        await db.SaveChangesAsync(ct);
        return new HighlightDto(highlight.Id, highlight.BookId, highlight.ChapterId, highlight.Text, highlight.StartOffset, highlight.EndOffset, highlight.Color, highlight.CreatedAt);
    }

    public async Task<IReadOnlyList<HighlightDto>> GetHighlightsAsync(Guid bookId, CancellationToken ct = default)
    {
        return await db.Highlights
            .Where(h => h.BookId == bookId)
            .OrderByDescending(h => h.CreatedAt)
            .Select(h => new HighlightDto(h.Id, h.BookId, h.ChapterId, h.Text, h.StartOffset, h.EndOffset, h.Color, h.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task DeleteHighlightAsync(Guid id, CancellationToken ct = default)
    {
        var highlight = await db.Highlights.FindAsync([id], ct);
        if (highlight is not null)
        {
            db.Highlights.Remove(highlight);
            await db.SaveChangesAsync(ct);
        }
    }
}
