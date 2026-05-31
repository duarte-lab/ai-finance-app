using Application.Notifications.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly GetDueNotificationsUseCase _getDueNotificationsUseCase;

    public NotificationsController(GetDueNotificationsUseCase getDueNotificationsUseCase)
    {
        _getDueNotificationsUseCase = getDueNotificationsUseCase;
    }

    [HttpGet("due")]
    public async Task<IActionResult> GetDueNotifications()
    {
        var notifications = await _getDueNotificationsUseCase.ExecuteAsync();
        return Ok(notifications);
    }
}