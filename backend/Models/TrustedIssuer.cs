namespace ShadowWallet.Models;

public class TrustedIssuer
{
    public int Id { get; set; }
    public required string Did { get; set; }
    public required string OrganizationName { get; set; }
    public string? Description { get; set; }
    public string? BadgeUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
}
