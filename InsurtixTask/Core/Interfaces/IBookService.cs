using InsurtixTask.Core.DTOs;
using InsurtixTask.Core.Enums;

namespace InsurtixTask.Core.Interfaces;

public interface IBookService
{
    IReadOnlyList<BookDTO> GetAllAvailableBooks();
    
    BookDTO TryGetBookByIsbn(string isbn);
    
    void TryAddNewBook(BookDTO book);
    
    BookDTO TryUpdateExistingBookByIsbn(BookDTO book);
    
    void TryDeleteExistingBookByIsbn(string isbn);

    public BooksReportDTO GenerateBooksReportAccordingToReportType(BooksReportType booksReportType,
        IReadOnlyList<BookDTO> allBooks);
}