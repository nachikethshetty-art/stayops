using Microsoft.EntityFrameworkCore;
using StayOps.Application.Common.Exceptions;
using StayOps.Application.Common.Interfaces;
using StayOps.Domain.Entities.Corporate;

namespace StayOps.Application.Corporate;

public class TravelAgentService(IApplicationDbContext db) : ITravelAgentService
{
    public async Task<IReadOnlyList<TravelAgentDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.TravelAgents.Select(ToDtoExpression).ToListAsync(ct);
    }

    public async Task<TravelAgentDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.TravelAgents.Where(a => a.Id == id).Select(ToDtoExpression).FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(TravelAgent), id);
    }

    public async Task<TravelAgentDto> CreateAsync(CreateTravelAgentRequest request, CancellationToken ct = default)
    {
        var agent = new TravelAgent
        {
            Name = request.Name,
            Gstin = request.Gstin,
            StateCode = request.StateCode,
            CommissionPercent = request.CommissionPercent
        };
        db.TravelAgents.Add(agent);
        await db.SaveChangesAsync(ct);
        return await GetByIdAsync(agent.Id, ct);
    }

    public async Task<TravelAgentDto> UpdateAsync(Guid id, UpdateTravelAgentRequest request, CancellationToken ct = default)
    {
        var agent = await db.TravelAgents.FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new NotFoundException(nameof(TravelAgent), id);

        agent.Name = request.Name;
        agent.Gstin = request.Gstin;
        agent.StateCode = request.StateCode;
        agent.CommissionPercent = request.CommissionPercent;
        agent.IsActive = request.IsActive;
        agent.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task<IReadOnlyList<AgentRateContractDto>> GetContractsForHotelAsync(Guid hotelId, CancellationToken ct = default)
    {
        return await db.AgentRateContracts.Where(c => c.HotelId == hotelId).Select(ToContractDtoExpression).ToListAsync(ct);
    }

    public async Task<AgentRateContractDto> CreateContractAsync(Guid hotelId, CreateAgentRateContractRequest request, CancellationToken ct = default)
    {
        var agentExists = await db.TravelAgents.AnyAsync(a => a.Id == request.TravelAgentId, ct);
        if (!agentExists) throw new NotFoundException(nameof(TravelAgent), request.TravelAgentId);

        var ratePlanValid = await db.RatePlans.AnyAsync(rp => rp.Id == request.RatePlanId && rp.HotelId == hotelId, ct);
        if (!ratePlanValid) throw new NotFoundException("RatePlan", request.RatePlanId);

        var contract = new AgentRateContract
        {
            TravelAgentId = request.TravelAgentId,
            HotelId = hotelId,
            RatePlanId = request.RatePlanId,
            ContractStart = request.ContractStart,
            ContractEnd = request.ContractEnd,
            DiscountPercent = request.DiscountPercent
        };
        db.AgentRateContracts.Add(contract);
        await db.SaveChangesAsync(ct);

        return await db.AgentRateContracts.Where(c => c.Id == contract.Id).Select(ToContractDtoExpression).FirstAsync(ct);
    }

    public async Task<AgentRateContractDto> UpdateContractAsync(Guid hotelId, Guid contractId, UpdateAgentRateContractRequest request, CancellationToken ct = default)
    {
        var contract = await db.AgentRateContracts.FirstOrDefaultAsync(c => c.Id == contractId && c.HotelId == hotelId, ct)
            ?? throw new NotFoundException(nameof(AgentRateContract), contractId);

        contract.ContractStart = request.ContractStart;
        contract.ContractEnd = request.ContractEnd;
        contract.DiscountPercent = request.DiscountPercent;
        contract.IsActive = request.IsActive;
        contract.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return await db.AgentRateContracts.Where(c => c.Id == contractId).Select(ToContractDtoExpression).FirstAsync(ct);
    }

    private static readonly System.Linq.Expressions.Expression<Func<TravelAgent, TravelAgentDto>> ToDtoExpression = a => new TravelAgentDto(
        a.Id, a.Name, a.Gstin, a.StateCode, a.CommissionPercent, a.IsActive);

    private static readonly System.Linq.Expressions.Expression<Func<AgentRateContract, AgentRateContractDto>> ToContractDtoExpression = c => new AgentRateContractDto(
        c.Id, c.TravelAgentId, c.TravelAgent!.Name, c.HotelId, c.RatePlanId, c.RatePlan!.Name,
        c.ContractStart, c.ContractEnd, c.DiscountPercent, c.IsActive);
}
