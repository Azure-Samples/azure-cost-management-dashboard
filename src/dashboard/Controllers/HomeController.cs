using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using services.APIs.CostManagement;
using services.Dtos;
using System.Threading.Tasks;

namespace dashboard.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel
            {
                WeeklyBillingList = await CostManagementService.GetWeeklyBillingAsync(),
                MonthToDateDtoList = await CostManagementService.GetMonthToDateBillingAsync()
            };

            ViewData["WeeklyBilling"] = model.WeeklyBillingList;
            ViewData["MonthToDate"] = model.MonthToDateDtoList;

            return View();
        }
    }
}
