using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace TwitchBot.Service.Pages
{
    public class PrivacyModel : PageModel
    {
        private readonly ILogger<PrivacyModel> _logger;

        public PrivacyModel(
            ILoggerFactory loggerFactory
            )
        {
            _logger = loggerFactory.CreateLogger<PrivacyModel>();
        }

        public void OnGet()
        {
        }
    }
}
