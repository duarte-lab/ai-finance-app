using Application.Auth.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Infrastructure.Auth;

public class HttpCurrentUserContext : ICurrentUserContext
{
    public Guid? TenantId { get; }
    public Guid? UserId { get; }

    public HttpCurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            if (Guid.TryParse(user.FindFirst("tenantId")?.Value, out var tid))
                TenantId = tid;

            if (Guid.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var uid))
                UserId = uid;
        }
    }
}
