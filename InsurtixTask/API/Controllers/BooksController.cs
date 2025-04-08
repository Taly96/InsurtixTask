using InsurtixTask.Core.DTOs;
using InsurtixTask.Core.Enums;
using InsurtixTask.Core.Exceptions;
using InsurtixTask.Core.Interfaces;

namespace InsurtixTask.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _xmlBooksService;
    
    private readonly  ILogger<BooksController> _logger;
    
    
    public BooksController(IBookService xmlBooksService, ILogger<BooksController> logger)
    {
        _xmlBooksService = xmlBooksService;
        _logger = logger;
    }

    /// <summary>
    /// Returns all available books
    /// </summary>
    /// <returns>A list of books. The list may be empty if no books are available.</returns>
    /// <response code="200">The list of books was returned successfully (can be empty)</response>
    /// <response code="400">Bad request</response>
    /// <response code="404">XML file Not found</response>
    /// <response code="500">Internal Server Error</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<IReadOnlyList<BookDTO>> GetAllAvailableBooks()
    {
        try
        {
            _logger.LogInformation("Getting all available books");
            IReadOnlyList<BookDTO> allAvailableBooks = _xmlBooksService.GetAllAvailableBooks();

            return Ok(allAvailableBooks);
        }
        catch (FileNotFoundException fileNotFoundException)
        {
            _logger.LogError(fileNotFoundException, "XML file could not be found while getting all books: {Message}", fileNotFoundException.Message);
            
            return NotFound(fileNotFoundException.Message);
        }
        catch (XElementToDTOParseException parseException)
        {
            _logger.LogError(parseException, "Xml element parsing error occurred while getting all books: {Message}", parseException.Message);
            
            return BadRequest( parseException.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unexpected error occurred getting all books: {Message}", exception.Message);
            
            return StatusCode(500, exception.Message);
        }
    }

    /// <summary>
    /// Returns a book by ISBN
    /// </summary>
    /// <param name="isbn">The identifier of the book</param>
    /// <returns>Book details</returns>
    /// <response code="200">Found</response>
    /// <response code="404">Not found</response>
    /// <response code="400">Bad request</response>
    /// <response code="500">Internal Server Error</response>
    [HttpGet("{isbn}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<BookDTO> GetBookByIsbn(string isbn)
    {
        try
        {
            _logger.LogInformation("Getting book by isbn: {isbn}", isbn);
            var book = _xmlBooksService.TryGetBookByIsbn(isbn);
            
            return  Ok(book);
        }
        catch (FileNotFoundException fileNotFoundException)
        {
            _logger.LogError(fileNotFoundException, "XML file could not be found while getting book by isbn {isbn} : {Message}", isbn, fileNotFoundException.Message);
            
            return NotFound(fileNotFoundException.Message);
        }
        catch (XElementToDTOParseException parseException)
        {
            _logger.LogError(parseException, "Xml element parsing error occurred while getting book by isbn {isbn} : {Message}", isbn, parseException.Message);
            
            return BadRequest( parseException.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unexpected error occurred getting book by isbn {isbn} : {Message}", isbn, exception.Message);
            
            return StatusCode(500, exception.Message);
        }
    }

    /// <summary>
    /// Creates a new book
    /// </summary>
    /// <param name="book">Book details</param>
    /// <returns>The created book</returns>
    /// <response code="201">Created</response>
    /// <response code="404">Not Found</response>
    /// <response code="400">Bad Request</response>
    /// <response code="409">Conflict</response>
    /// <response code="500">Internal Server Error</response>
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult AddNewBook(BookDTO book)
    {
        try
        {
            _logger.LogInformation("Adding new book: {Isbn}", book.Isbn);
            _xmlBooksService.TryAddNewBook(book);

            return Ok(book);
        }
        catch (FileNotFoundException fileNotFoundException)
        {
            _logger.LogError(fileNotFoundException, "XML file could not be found while adding new book : {Message}", fileNotFoundException.Message);
            
            return NotFound(fileNotFoundException.Message);
        }
        catch (BookDTOToXElementParseException parseException)
        {
            _logger.LogError(parseException, "Book DTO parsing error occurred while adding new book: {Message}", parseException.Message);
            
            return BadRequest( parseException.Message);
        }
        catch (BookExistsException existsException)
        {
            _logger.LogError(existsException, "An error occurred while adding new book: {Message}", existsException.Message);
            
            return Conflict( existsException.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unexpected error occurred adding new book : {Message}", exception.Message);
            
            return StatusCode(500, exception.Message);
        }
    }

    /// <summary>
    /// Updates an existing book by ISBN with the provided data and return the updated book.
    /// </summary>
    /// <response code="200">Successfully updated</response>
    /// <response code="400">Invalid input or parsing error</response>
    /// <response code="404">Book or file not found</response>
    /// <response code="500">Unexpected server error</response>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult Update(BookDTO book)
    {
        try
        {
            _logger.LogInformation("Updating book: {Isbn}", book.Isbn);
            BookDTO updatedBook = _xmlBooksService.TryUpdateExistingBookByIsbn(book);

            return Ok(updatedBook);
        }
        catch (ArgumentNullException nullException)
        {
            _logger.LogError(nullException, "There was a problem with the book input while updating a book : {Message}", nullException.Message);
            
            return BadRequest(nullException.Message);
        }
        catch (ArgumentException argumentException)
        {
            _logger.LogError(argumentException, "Could not find book for update : {Message}", argumentException.Message);
            
            return NotFound(argumentException.Message);
        }
        catch (FileNotFoundException fileNotFoundException)
        {
            _logger.LogError(fileNotFoundException, "XML file could not be found while updating a book : {Message}", fileNotFoundException.Message);
            
            return NotFound(fileNotFoundException.Message);
        }
        catch (BookDTOToXElementParseException parseException)
        {
            _logger.LogError(parseException, "Book DTO parsing error occurred while updating a book: {Message}", parseException.Message);
            
            return BadRequest( parseException.Message);
        }
        catch (XElementToDTOParseException parseException)
        {
            _logger.LogError(parseException, "Xml element parsing error occurred while updating a book : {Message}", parseException.Message);
            
            return BadRequest( parseException.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unexpected error occurred while updating a book : {Message}", exception.Message);
            
            return StatusCode(500, exception.Message);
        }
    }

    /// <summary>
    /// Deletes an existing book by its ISBN.
    /// </summary>
    /// <param name="isbn">The identifier (ISBN) of the book to delete</param>
    /// <response code="204">Successfully deleted</response>
    /// <response code="400">Invalid input</response>
    /// <response code="404">Book or file not found</response>
    /// <response code="500">Unexpected server error</response>
    [HttpDelete("{isbn}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult Delete(string isbn)
    {
        try
        {
            _logger.LogInformation($"Deleting book : {isbn}");
            _xmlBooksService.TryDeleteExistingBookByIsbn(isbn);
            
            return NoContent();
        }
        catch (ArgumentNullException nullException)
        {
            _logger.LogError(nullException, "There was a problem with isbn input while deleting a book : {Message}", nullException.Message);
            
            return BadRequest(nullException.Message);
        }
        catch (ArgumentException argumentException)
        {
            _logger.LogError(argumentException, "Could not find book to delete : {Message}", argumentException.Message);
            
            return NotFound(argumentException.Message);
        }
        catch (FileNotFoundException fileNotFoundException)
        {
            _logger.LogError(fileNotFoundException, "XML file could not be found while deleting a book : {Message}", fileNotFoundException.Message);
            
            return NotFound(fileNotFoundException.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unexpected error occurred while deleting a book : {Message}", exception.Message);
            
            return StatusCode(500, exception.Message);
        }
    }

    /// <summary>
    /// Generates and returns a report of all available books in the specified format.
    /// </summary>
    /// <param name="booksReportType">The desired report type (e.g., HTML, CSV).</param>
    /// <returns>A content result containing the generated report.</returns>
    /// <response code="200">The report was generated successfully.</response>
    /// <response code="400">Invalid data or error parsing XML elements.</response>
    /// <response code="404">No books found or XML file not found.</response>
    /// <response code="500">An unexpected error occurred while generating the report.</response>
    [HttpGet("report")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult GetReport(BooksReportType booksReportType)
    {
        try
        {   _logger.LogInformation($"Generating report type {booksReportType}");
            IReadOnlyList<BookDTO> allBooks = _xmlBooksService.GetAllAvailableBooks();
            BooksReportDTO booksReportDto = _xmlBooksService.GenerateBooksReportAccordingToReportType(booksReportType, allBooks);
            
            return new ContentResult
            {
                Content = booksReportDto.Content,
                ContentType = booksReportDto.ContentType
            };
        }
        catch (FileNotFoundException fileNotFoundException)
        {
            _logger.LogError(fileNotFoundException, "XML file could not be found while generating report - {booksReportType}: {Message}", booksReportType, fileNotFoundException.Message);
            
            return NotFound(fileNotFoundException.Message);
        }
        catch (XElementToDTOParseException parseException)
        {
            _logger.LogError(parseException, "Xml element parsing error occurred while generating report - {booksReportType}: {Message}", booksReportType, parseException.Message);
            
            return BadRequest( parseException.Message);
        }
        catch (ArgumentException argumentException)
        {
            _logger.LogError(argumentException, "No books found to generate report - {booksReportType}: {Message}", booksReportType, argumentException.Message);
            
            return NotFound(argumentException.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unexpected error occurred while generating report - {booksReportType}: {Message}", booksReportType, exception.Message);
            
            return StatusCode(500, exception.Message);
        }
    }
}