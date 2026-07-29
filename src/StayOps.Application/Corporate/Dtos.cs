namespace StayOps.Application.Corporate;

public record CompanyDto(Guid Id, string Name, string Gstin, string StateCode, string BillingAddress, decimal CreditLimit, bool IsActive);
public record CreateCompanyRequest(string Name, string Gstin, string StateCode, string BillingAddress, decimal CreditLimit);
public record UpdateCompanyRequest(string Name, string Gstin, string StateCode, string BillingAddress, decimal CreditLimit, bool IsActive);

public record CorporateRateContractDto(
    Guid Id, Guid CompanyId, string CompanyName, Guid HotelId, Guid RatePlanId, string RatePlanName,
    DateOnly ContractStart, DateOnly ContractEnd, decimal? DiscountPercent, bool BillToCompanyByDefault, bool IsActive);
public record CreateCorporateRateContractRequest(Guid CompanyId, Guid RatePlanId, DateOnly ContractStart, DateOnly ContractEnd, decimal? DiscountPercent, bool BillToCompanyByDefault);
public record UpdateCorporateRateContractRequest(DateOnly ContractStart, DateOnly ContractEnd, decimal? DiscountPercent, bool BillToCompanyByDefault, bool IsActive);

public record TravelAgentDto(Guid Id, string Name, string Gstin, string StateCode, decimal CommissionPercent, bool IsActive);
public record CreateTravelAgentRequest(string Name, string Gstin, string StateCode, decimal CommissionPercent);
public record UpdateTravelAgentRequest(string Name, string Gstin, string StateCode, decimal CommissionPercent, bool IsActive);

public record AgentRateContractDto(
    Guid Id, Guid TravelAgentId, string TravelAgentName, Guid HotelId, Guid RatePlanId, string RatePlanName,
    DateOnly ContractStart, DateOnly ContractEnd, decimal? DiscountPercent, bool IsActive);
public record CreateAgentRateContractRequest(Guid TravelAgentId, Guid RatePlanId, DateOnly ContractStart, DateOnly ContractEnd, decimal? DiscountPercent);
public record UpdateAgentRateContractRequest(DateOnly ContractStart, DateOnly ContractEnd, decimal? DiscountPercent, bool IsActive);
