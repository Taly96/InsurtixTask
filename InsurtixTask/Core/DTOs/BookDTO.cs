namespace InsurtixTask.Core.DTOs;

public class BookDTO
{
    public string Isbn { get; set; } = "";
    
    public string Title { get; set; } = "";
    
    public List<string> Authors { get; set; } = [];
    
    public string Category { get; set; } = "";

    public int Year { get; set; }

    public decimal Price { get; set; } = -1;
}