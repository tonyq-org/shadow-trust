using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShadowTrust.Data;

namespace ShadowTrust.Controllers;

[ApiController]
[Route("api/trust")]
public class TrustController : ControllerBase
{
    private readonly AppDbContext _db;

    public TrustController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("{did}")]
    public async Task<IActionResult> GetTrustStatus(string did)
    {
        var issuer = await _db.TrustedIssuers
            .FirstOrDefaultAsync(i => i.Did == did && i.IsActive);

        if (issuer == null || (issuer.ExpiresAt.HasValue && issuer.ExpiresAt < DateTime.UtcNow))
        {
            return Ok(new { trusted = false });
        }

        return Ok(new
        {
            trusted = true,
            organizationName = issuer.OrganizationName,
            badgeUrl = issuer.BadgeUrl
        });
    }
}
