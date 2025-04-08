using System.Xml.Linq;
using InsurtixTask.Core.AbstractClasses;
using InsurtixTask.Core.Constants;
using InsurtixTask.Core.DTOs;
using InsurtixTask.Core.Exceptions;

namespace InsurtixTask.Core.Services;

internal class XmlBooksService : BooksServiceBase
{
    private readonly string _xmlFilePath;

    public XmlBooksService(IConfiguration configuration)
    {
        _xmlFilePath = configuration["XmlFilePath"] ?? Settings.XmlDefaultPath;
    }

    public override IReadOnlyList<BookDTO> GetAllAvailableBooks()
    {
        IReadOnlyList<BookDTO> allBooks;
        
        if (!string.IsNullOrEmpty(_xmlFilePath) && File.Exists(_xmlFilePath))
        {
            var xmlBooksDocument = XDocument.Load(_xmlFilePath);

            allBooks = xmlBooksDocument.Descendants(Settings.BookElement)
                .Select(tryParseXElementToBookDTO)
                .ToList();
        }
        else
        {
            throw new FileNotFoundException($"XML file could not be found. File path: {_xmlFilePath}", _xmlFilePath);
        }

        return allBooks;
    }

    public override BookDTO TryGetBookByIsbn(string isbn)
    {
        BookDTO bookDTOByIsbn;
        
        if (!string.IsNullOrEmpty(_xmlFilePath) && File.Exists(_xmlFilePath))
        {
            var xmlBooksDocument = XDocument.Load(_xmlFilePath);
            var bookElement = xmlBooksDocument.Descendants(Settings.BookElement)
                .FirstOrDefault(book => (string)book.Element(Settings.IsbnElement)! == isbn);

            bookDTOByIsbn = bookElement != null ? tryParseXElementToBookDTO(bookElement) : throw new Exception($"Could not find book by isbn: {isbn}");
        }
        else
        {
            throw new FileNotFoundException($"XML file could not be found. File path: {_xmlFilePath}", _xmlFilePath);
        }
        
        return bookDTOByIsbn;
    }

    public override void TryAddNewBook(BookDTO newBook)
    {
        if (!string.IsNullOrEmpty(_xmlFilePath) && File.Exists(_xmlFilePath))
        {
            var newBookElem = tryCreateBookXElementFromBookDTO(newBook);
            var xmlBooksDocument = XDocument.Load(_xmlFilePath);

            if (xmlBooksDocument.Descendants(Settings.BookElement)
                .Any(book => (string)book.Element(Settings.IsbnElement)! == newBook.Isbn))
            {
                throw new BookExistsException($"Book with same ISBN:{newBook.Isbn} already exists");
            }

            xmlBooksDocument.Root!.Add(newBookElem);
            xmlBooksDocument.Save(_xmlFilePath);
        }
        else
        {
            throw new FileNotFoundException($"XML file could not be found. File path: {_xmlFilePath}", _xmlFilePath);
        }
    }
    
    public override BookDTO TryUpdateExistingBookByIsbn(BookDTO bookDataToUpdate)
    {   
        if (bookDataToUpdate == null)
        {
            throw new ArgumentNullException(nameof(bookDataToUpdate), "Book cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(bookDataToUpdate.Isbn))
        {
            throw new ArgumentNullException(nameof(bookDataToUpdate.Isbn), "ISBN cannot be null or empty.");
        }

        if (!string.IsNullOrEmpty(_xmlFilePath) && File.Exists(_xmlFilePath))
        {
            var xmlBooksDocument = XDocument.Load(_xmlFilePath);
            var bookElement = xmlBooksDocument.Descendants(Settings.BookElement)
                .FirstOrDefault(book => (string)book.Element(Settings.IsbnElement)! == bookDataToUpdate.Isbn);

            if (bookElement == null)
            {
                throw new ArgumentException($"Book not found by ISBN:{bookDataToUpdate.Isbn}");
            }
            
            var bookDTOFromExistingElement = validateBookXElementAndReturnValidBookDTO(bookElement);
            var updatedBookDTO = new BookDTO
            {
                Isbn = bookDataToUpdate.Isbn,
                Title = string.IsNullOrWhiteSpace(bookDataToUpdate.Title)
                    ? bookDTOFromExistingElement.Title
                    : bookDataToUpdate.Title,
                Price = bookDataToUpdate.Price > 0 ? bookDataToUpdate.Price : bookDTOFromExistingElement.Price,
                Category = string.IsNullOrWhiteSpace(bookDataToUpdate.Category)
                    ? bookDTOFromExistingElement.Category : bookDataToUpdate.Category ,
                Authors = bookDataToUpdate.Authors.Count > 0
                    ? bookDataToUpdate.Authors
                    : bookDTOFromExistingElement.Authors,
                Year = bookDataToUpdate.Year > 0 ? bookDataToUpdate.Year : bookDTOFromExistingElement.Year,
            };
            XElement updatedBookElement = tryCreateBookXElementFromBookDTO(updatedBookDTO);
            
            bookElement.ReplaceWith(updatedBookElement);
            xmlBooksDocument.Save(_xmlFilePath);

            return updatedBookDTO;
        }
        else
        {
            throw new FileNotFoundException($"XML file could not be found. File path: {_xmlFilePath}", _xmlFilePath);
        }
    }
    
    public override void TryDeleteExistingBookByIsbn(string isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
        {
            throw new ArgumentNullException(nameof(isbn), "ISBN cannot be null or empty.");
        }

        if (!string.IsNullOrEmpty(_xmlFilePath) && File.Exists(_xmlFilePath))
        {
            var xmlBooksDocument = XDocument.Load(_xmlFilePath);
            var bookElement = xmlBooksDocument.Descendants(Settings.BookElement)
                .FirstOrDefault(book => (string)book.Element(Settings.IsbnElement)! == isbn);

            if (bookElement == null)
                throw new ArgumentException($"Book not found by ISBN:{isbn}");

            bookElement.Remove();
            xmlBooksDocument.Save(_xmlFilePath);
        }
        else
        {
            throw new FileNotFoundException($"XML file could not be found. File path: {_xmlFilePath}", _xmlFilePath);
        }
    }

    private void validateBookDTO(BookDTO bookDTO)
    {
        if (bookDTO == null)
        {
            throw new BookDTOToXElementParseException("Book cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(bookDTO.Isbn))
        {
            throw new BookDTOToXElementParseException("ISBN cannot be null or empty.");
        }

        if (string.IsNullOrWhiteSpace(bookDTO.Title))
        {
            throw new BookDTOToXElementParseException("Title cannot be null or empty.");
        }

        if (string.IsNullOrWhiteSpace(bookDTO.Category))
        {
            throw new BookDTOToXElementParseException("Category cannot be null or empty.");
        }

        if (bookDTO.Authors == null || bookDTO.Authors.Count == 0 || bookDTO.Authors.Any(string.IsNullOrWhiteSpace))
        {
            throw new BookDTOToXElementParseException("Authors cannot be null, empty, or contain null/empty values.");
        }

        if (bookDTO.Year <= 0)
        {
            throw new BookDTOToXElementParseException("Year must be a positive number.");
        }

        if (bookDTO.Price < 0)
        {
            throw new BookDTOToXElementParseException("Price cannot be negative.");
        }
    }

    private BookDTO validateBookXElementAndReturnValidBookDTO(XElement bookElement)
    {
        if (bookElement == null)
        {
            throw new XElementToDTOParseException("Book xelement is null.");
        }

        string isbn = bookElement.Element(Settings.IsbnElement)?.Value 
                      ?? throw new XElementToDTOParseException("Missing required ISBN element.");
        string title = bookElement.Element(Settings.TitleElement)?.Value 
                       ?? throw new XElementToDTOParseException("Missing required title element.");
        var authorElements = bookElement.Elements(Settings.AuthorElement).ToList();

        if (authorElements.Count == 0)
        {
            throw new XElementToDTOParseException("Missing required authors elements.");
        }

        List<string> authors = authorElements.Select(elements => elements.Value).ToList();
        string category = bookElement.Attribute(Settings.CategoryElement)?.Value ?? "";
        string yearStr = bookElement.Element(Settings.YearElement)?.Value 
                         ?? throw new XElementToDTOParseException("Missing required year element.");

        if (!int.TryParse(yearStr, out int year))
        {
            throw new XElementToDTOParseException($"Invalid year value: '{yearStr}'");
        }

        string priceStr = bookElement.Element(Settings.PriceElement)?.Value 
                          ?? throw new XElementToDTOParseException("Missing required price element.");

        if (!decimal.TryParse(priceStr, out decimal price))
        {
            throw new XElementToDTOParseException($"Invalid price value: '{priceStr}'");
        }

        return new BookDTO
        {
            Isbn = isbn,
            Title = title,
            Authors = authors,
            Category = category,
            Year = year,
            Price = price
        };
    }
    private BookDTO tryParseXElementToBookDTO(XElement bookElement)
    {
        var validBookDTO = validateBookXElementAndReturnValidBookDTO(bookElement);

        return validBookDTO;
    }

    private XElement tryCreateBookXElementFromBookDTO(BookDTO bookDTO)
    {
        validateBookDTO(bookDTO);

        return new XElement(Settings.BookElement,
            new XAttribute(Settings.CategoryElement, bookDTO.Category),
            new XElement(Settings.IsbnElement, bookDTO.Isbn),
            new XElement(Settings.TitleElement, bookDTO.Title),
            bookDTO.Authors.Select(author => new XElement(Settings.AuthorElement, author)),
            new XElement(Settings.YearElement, bookDTO.Year),
            new XElement(Settings.PriceElement, bookDTO.Price)
        );
    }
}
