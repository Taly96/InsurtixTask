using InsurtixTask.Core.DTOs;
using InsurtixTask.Core.Enums;
using InsurtixTask.Core.Interfaces;
using InsurtixTask.Core.Models;

namespace InsurtixTask.Core.AbstractClasses;

internal abstract class BooksServiceBase : IBookService
{
    private readonly BooksReportGenerator _reportGenerator = new ();
    
    public virtual BooksReportDTO GenerateBooksReportAccordingToReportType(BooksReportType booksReportType, IReadOnlyList<BookDTO> allBooks)
    {
        IBooksReport booksReport = _reportGenerator.GenerateReport(booksReportType, allBooks);
        string contentType = booksReportType switch
        {
            BooksReportType.HTML => "text/html",
            _ => "application/octet-stream"
        };

        return new BooksReportDTO(contentType, booksReport.GetReportString());
    }

    public abstract IReadOnlyList<BookDTO> GetAllAvailableBooks();
    
    public abstract BookDTO TryGetBookByIsbn(string isbn);
    
    public abstract void TryAddNewBook(BookDTO book);
    
    public abstract BookDTO TryUpdateExistingBookByIsbn(BookDTO book);
    
    public abstract void TryDeleteExistingBookByIsbn(string isbn);
}