using Microsoft.EntityFrameworkCore;
using ContractorMonitoring.Application.Interfaces;
using ContractorMonitoring.Domain.Entities;
using ContractorMonitoring.Infrastructure.Data;

namespace ContractorMonitoring.Infrastructure.Repositories;

public class ApprovalRepository : IApprovalRepository
{
    private readonly ApplicationDbContext _context;

    public ApprovalRepository(ApplicationDbContext context) => _context = context;

    public async Task<ApprovalWorkflow?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.ApprovalWorkflows.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public Task UpdateAsync(ApprovalWorkflow approval, CancellationToken cancellationToken = default)
    {
        _context.ApprovalWorkflows.Update(approval);
        // SaveChanges is the caller's responsibility — do NOT call it here
        return Task.CompletedTask;
    }
}
