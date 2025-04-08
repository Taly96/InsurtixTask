using System.Text;
using System.Web;
using InsurtixTask.Core.DTOs;
using InsurtixTask.Core.Enums;
using InsurtixTask.Core.Interfaces;

namespace InsurtixTask.Core.Models;

internal class BooksReportGenerator
{
    public IBooksReport GenerateReport(BooksReportType booksReportType, IReadOnlyList<BookDTO> allBooks)
    {
        IBooksReport report = null;
        
        switch (booksReportType)
        {
            case BooksReportType.HTML:
                report = generateHtmlReport(allBooks);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(booksReportType), booksReportType, null);
        }

        return report;
    }

    private HtmlReport generateHtmlReport(IReadOnlyList<BookDTO> allBooks)
    {
        if (allBooks == null || allBooks.Count == 0)
        {
            throw new ArgumentException("No books available to generate the report.", nameof(allBooks));
        }

        var sb = new StringBuilder();

        foreach (var book in allBooks)
        {
            var authors = book.Authors != null && book.Authors.Any() 
                ? string.Join(", ", book.Authors) 
                : "Unknown Author";

            sb.Append($"<tr><td>{HttpUtility.HtmlEncode(book.Title)}</td>" +
                      $"<td>{HttpUtility.HtmlEncode(authors)}</td>" +
                      $"<td>{HttpUtility.HtmlEncode(book.Category)}</td>" +
                      $"<td>{HttpUtility.HtmlEncode(book.Year.ToString())}</td>" +
                      $"<td>{HttpUtility.HtmlEncode(book.Price.ToString())}</td></tr>");
        }

        return new HtmlReport($"<table border='1' style='border-collapse: collapse;'>" +
                              "<thead><tr><th>Title</th><th>Authors</th><th>Category</th><th>Year</th><th>Price</th></tr></thead>" +
                              $"<tbody>{sb.ToString()}</tbody></table>");
    }
}