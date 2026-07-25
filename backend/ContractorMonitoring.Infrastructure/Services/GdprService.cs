using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ContractorMonitoring.Application.Interfaces;
using ContractorMonitoring.Infrastructure.Data;

namespace ContractorMonitoring.Infrastructure.Services;

public class GdprService : IGdprService
{
    private readonly ApplicationDbContext _context;

    public GdprService(ApplicationDbContext context) => _context = context;

    public async Task<string> ExportUserDataAsync(Guid userId)
    {
        var user = await _context.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) throw new InvalidOperationException("User not found");

        var roles = await _context.UserRoles.IgnoreQueryFilters()
            .Where(ur => ur.UserId == userId)
            .Join(_context.Roles.IgnoreQueryFilters(), ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
            .ToListAsync();

        var activityLogs = await _context.UserActivityLogs.IgnoreQueryFilters()
            .Where(l => l.UserId == userId)
            .Select(l => new { l.ActivityType, l.ModuleName, l.Action, l.IpAddress, l.CreatedAt })
            .Take(1000)
            .ToListAsync();

        var export = new
        {
            ExportedAt = DateTime.UtcNow,
            User = new
            {
                user.Id, user.Email, user.FirstName, user.LastName,
                user.PhoneNumber, user.IsActive, user.CreatedAt, user.LastLoginAt,
                user.Department, user.JobTitle, user.Company, user.Timezone, user.Language
            },
            Roles = roles,
            ActivityLogs = activityLogs
        };

        return JsonSerializer.Serialize(export, new JsonSerializerOptions { WriteIndented = true });
    }

    public async Task EraseUserDataAsync(Guid userId, string erasedBy)
    {
        var user = await _context.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return;

        // Anonymise PII — do NOT hard-delete (preserve audit trail)
        user.Email = $"erased_{userId}@gdpr.deleted";
        user.FirstName = "ERASED";
        user.LastName = "ERASED";
        user.PhoneNumber = null;
        user.ProfilePicture = null;
        user.Bio = null;
        user.JobTitle = null;
        user.Department = null;
        user.Company = null;
        user.PasswordHash = "ERASED";
        user.RefreshToken = null;
        user.TwoFactorSecret = null;
        user.SecurityAnswerHash = null;
        user.IsActive = false;
        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = erasedBy;

        // Anonymise activity logs
        await _context.UserActivityLogs.IgnoreQueryFilters()
            .Where(l => l.UserId == userId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(l => l.UserEmail, "erased@gdpr.deleted")
                .SetProperty(l => l.UserName, "ERASED")
                .SetProperty(l => l.IpAddress, "0.0.0.0")
                .SetProperty(l => l.RequestBody, (string?)null));

        await _context.SaveChangesAsync();
    }
}
