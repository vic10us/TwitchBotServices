using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace TwitchBot.Service.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            ILoggerFactory loggerFactory
            )
        {
            _logger = loggerFactory.CreateLogger<IndexModel>();
        }

        public void OnGet()
        {

        }
    }
}
