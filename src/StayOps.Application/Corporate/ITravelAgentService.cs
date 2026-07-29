namespace StayOps.Application.Corporate;

public interface ITravelAgentService
{
    Task<IReadOnlyList<TravelAgentDto>> GetAllAsync(CancellationToken ct = default);
    Task<TravelAgentDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<TravelAgentDto> CreateAsync(CreateTravelAgentRequest request, CancellationToken ct = default);
    Task<TravelAgentDto> UpdateAsync(Guid id, UpdateTravelAgentRequest request, CancellationToken ct = default);

    Task<IReadOnlyList<AgentRateContractDto>> GetContractsForHotelAsync(Guid hotelId, CancellationToken ct = default);
    Task<AgentRateContractDto> CreateContractAsync(Guid hotelId, CreateAgentRateContractRequest request, CancellationToken ct = default);
    Task<AgentRateContractDto> UpdateContractAsync(Guid hotelId, Guid contractId, UpdateAgentRateContractRequest request, CancellationToken ct = default);
}
