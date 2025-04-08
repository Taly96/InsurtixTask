
namespace InsurtixTask.Core.DTOs;

public class BooksReportDTO
{
    public string ContentType { get; set; }
    
    public string Content { get; set; }
    
    
    public BooksReportDTO(string contentType, string content)
    {
        ContentType = contentType;
        Content = content;
    }
}