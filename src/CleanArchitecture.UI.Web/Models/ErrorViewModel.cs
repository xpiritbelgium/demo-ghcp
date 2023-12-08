using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CleanArchitecture.UI.Web.Models
{
    public class ErrorViewModel : PageModel
    {
        public ErrorViewModel()
        {
            Errors = new List<string>();
        }

        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public string? ExceptionMessage { get; set; }

        public List<string>? Errors { get; set; }

        public string Stacktrace { get; set; }
    }
}