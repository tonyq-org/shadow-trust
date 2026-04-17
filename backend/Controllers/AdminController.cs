using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShadowTrust.Data;
using ShadowTrust.Models;

namespace ShadowTrust.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("issuers")]
    public async Task<IActionResult> ListIssuers()
    {
        var issuers = await _db.TrustedIssuers.OrderByDescending(i => i.CreatedAt).ToListAsync();
        return Ok(issuers);
    }

    [HttpGet("issuers/{id}")]
    public async Task<IActionResult> GetIssuer(int id)
    {
        var issuer = await _db.TrustedIssuers.FindAsync(id);
        if (issuer == null) return NotFound();
        return Ok(issuer);
    }

    [HttpPost("issuers")]
    public async Task<IActionResult> CreateIssuer([FromBody] TrustedIssuer issuer)
    {
        issuer.CreatedAt = DateTime.UtcNow;
        _db.TrustedIssuers.Add(issuer);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetIssuer), new { id = issuer.Id }, issuer);
    }

    [HttpPut("issuers/{id}")]
    public async Task<IActionResult> UpdateIssuer(int id, [FromBody] TrustedIssuer updated)
    {
        var issuer = await _db.TrustedIssuers.FindAsync(id);
        if (issuer == null) return NotFound();

        issuer.Did = updated.Did;
        issuer.OrganizationName = updated.OrganizationName;
        issuer.Description = updated.Description;
        issuer.BadgeUrl = updated.BadgeUrl;
        issuer.IsActive = updated.IsActive;
        issuer.ExpiresAt = updated.ExpiresAt;

        await _db.SaveChangesAsync();
        return Ok(issuer);
    }

    [HttpDelete("issuers/{id}")]
    public async Task<IActionResult> DeleteIssuer(int id)
    {
        var issuer = await _db.TrustedIssuers.FindAsync(id);
        if (issuer == null) return NotFound();

        _db.TrustedIssuers.Remove(issuer);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
