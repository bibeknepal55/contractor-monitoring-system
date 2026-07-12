using ContractorMonitoring.Domain.Entities;

namespace ContractorMonitoring.Application.Interfaces;

public interface IApprovalRepository
{
    Task<ApprovalWorkflow?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(ApprovalWorkflow approval, CancellationToken cancellationToken = default);
}