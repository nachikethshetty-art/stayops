using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using StayOps.Application.CancellationPolicies;
using StayOps.Application.Corporate;
using StayOps.Application.Guests;
using StayOps.Application.GstRules;
using StayOps.Application.Hotels;
using StayOps.Application.Housekeeping;
using StayOps.Application.Inventory;
using StayOps.Application.Rates;
using StayOps.Application.Reservations;

namespace StayOps.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IHotelGroupService, HotelGroupService>();
        services.AddScoped<IHotelService, HotelService>();
        services.AddScoped<IRoomTypeService, RoomTypeService>();
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IGuestService, GuestService>();
        services.AddScoped<IHousekeepingService, HousekeepingService>();
        services.AddScoped<IPaymentWebhookService, PaymentWebhookService>();
        services.AddScoped<IRatePlanService, RatePlanService>();
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<ITravelAgentService, TravelAgentService>();
        services.AddScoped<IRefundService, RefundService>();
        services.AddScoped<IRefundCompletionService, RefundCompletionService>();
        services.AddScoped<ICancellationPolicyService, CancellationPolicyService>();
        services.AddScoped<IGstRuleService, GstRuleService>();

        return services;
    }
}
