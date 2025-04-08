using InsurtixTask.Core.Interfaces;

namespace InsurtixTask.Core.Models;

internal class HtmlReport : IBooksReport
{
    private string _htmlReport;

    public HtmlReport(string htmlReport)
    {
        _htmlReport = htmlReport;
    }
    
    public string GetReportString()
    {
        return _htmlReport;
    }
}