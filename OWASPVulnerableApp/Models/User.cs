namespace OWASPVulnerableApp.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty; // Vulnerability: stored in plaintext
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "user";
    public bool IsActive { get; set; } = true;
    public string SensitiveData { get; set; } = string.Empty; // Vulnerability: no encryption
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
