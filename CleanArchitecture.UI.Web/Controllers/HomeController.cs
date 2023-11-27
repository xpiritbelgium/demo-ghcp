using Azure.Core;
using CleanArchitecture.Application.Exceptions;
using CleanArchitecture.Application.Extensions;
using CleanArchitecture.Application.HealthChecks;
using CleanArchitecture.UI.Web.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CleanArchitecture.UI.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMediator _mediator;

        public HomeController(ILogger<HomeController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var exceptionHandlerPathFeature =
                HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            string message;
            var errors = new List<string>();

            switch (exceptionHandlerPathFeature?.Error)
            {
                case ValidationException validationException:
                    message = validationException.Message;
                    errors = validationException.ValdationErrors;
                    break;
                case BadRequestException badRequestException:
                    message = badRequestException.Message;
                    break;
                case NotFoundException notFoundexception:
                    message = notFoundexception.Message;
                    break;
                default:
                    message = "Unknow error";
                    break;
            }

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier, ExceptionMessage= message, Errors = errors, Stacktrace = exceptionHandlerPathFeature?.Error?.StackTrace ?? string.Empty });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> HealthCheck()
        {
            var id = await _mediator.Send(new CreateHealthCheckCommand("kluyten@xpirit.com"));
            return View(id);
        }
    }
}