using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StayOps.Application.Common;
using StayOps.Domain.Entities.Billing;
using StayOps.Domain.Entities.CancellationPolicies;
using StayOps.Domain.Entities.Corporate;
using StayOps.Domain.Entities.Guests;
using StayOps.Domain.Entities.Identity;
using StayOps.Domain.Entities.Inventory;
using StayOps.Domain.Entities.Organization;
using StayOps.Domain.Entities.Pos;
using StayOps.Domain.Entities.Rates;
using StayOps.Domain.Enums;
using StayOps.Infrastructure.Identity;

namespace StayOps.Infrastructure.Persistence.Seed;

/// <summary>
/// Idempotent demo/reference data: hotel group, 2 hotels in different GST states (to demo both
/// CGST+SGST and IGST), room types/rooms, GST rule slabs, cancellation policy, rate plans, a
/// corporate account + contract, a travel agent + contract, a POS outlet, demo guests, and one
/// seeded user per role (see root README "Demo credentials" for the login list).
/// DISCLAIMER: GST rates/slabs and GSTIN formats below are illustrative demo data only, not
/// verified legal tax advice - see README limitations.
/// </summary>
public static class DemoDataSeeder
{
    private const string DemoPassword = "Passw0rd!123";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var db = serviceProvider.GetRequiredService<ApplicationDbContext>();

        if (await db.HotelGroups.AnyAsync())
        {
            return; // already seeded
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // ---- Hotel group + hotels ----------------------------------------------------------
        var group = new HotelGroup { Name = "StayOps Demo Hospitality Group" };
        db.HotelGroups.Add(group);

        var hotelMumbai = new Hotel
        {
            HotelGroupId = group.Id,
            Code = "MUM01",
            Name = "StayOps Mumbai Business Bay",
            AddressLine1 = "Plot 12, Bandra Kurla Complex",
            City = "Mumbai",
            Pincode = "400051",
            StateCode = "27",
            StateName = "Maharashtra",
            Gstin = "27AAAPS1234F1Z5",
            TimeZoneId = "Asia/Kolkata",
            BusinessDate = today.AddDays(-1)
        };
        var hotelBangalore = new Hotel
        {
            HotelGroupId = group.Id,
            Code = "BLR01",
            Name = "StayOps Bangalore Tech Park",
            AddressLine1 = "Outer Ring Road, Marathahalli",
            City = "Bangalore",
            Pincode = "560103",
            StateCode = "29",
            StateName = "Karnataka",
            Gstin = "29AAAPS1234F1Z4",
            TimeZoneId = "Asia/Kolkata",
            BusinessDate = today.AddDays(-1)
        };
        db.Hotels.AddRange(hotelMumbai, hotelBangalore);

        // ---- Room types + rooms (per hotel) ------------------------------------------------
        var allRoomsByHotel = new Dictionary<Guid, List<Room>>();
        var roomTypesByHotel = new Dictionary<Guid, List<RoomType>>();

        foreach (var hotel in new[] { hotelMumbai, hotelBangalore })
        {
            var standard = new RoomType { HotelId = hotel.Id, Code = "STD", Name = "Standard Room", BaseOccupancy = 2, MaxOccupancy = 2, MaxChildren = 1, Description = "Comfortable standard room with city view." };
            var deluxe = new RoomType { HotelId = hotel.Id, Code = "DLX", Name = "Deluxe Room", BaseOccupancy = 2, MaxOccupancy = 3, MaxChildren = 1, Description = "Spacious deluxe room with premium amenities." };
            var suite = new RoomType { HotelId = hotel.Id, Code = "STE", Name = "Executive Suite", BaseOccupancy = 2, MaxOccupancy = 4, MaxChildren = 2, Description = "Suite with separate living area." };
            db.RoomTypes.AddRange(standard, deluxe, suite);
            roomTypesByHotel[hotel.Id] = [standard, deluxe, suite];

            var rooms = new List<Room>();
            for (var i = 1; i <= 6; i++) rooms.Add(new Room { HotelId = hotel.Id, RoomTypeId = standard.Id, RoomNumber = $"1{i:00}", Floor = "1" });
            for (var i = 1; i <= 4; i++) rooms.Add(new Room { HotelId = hotel.Id, RoomTypeId = deluxe.Id, RoomNumber = $"2{i:00}", Floor = "2" });
            for (var i = 1; i <= 2; i++) rooms.Add(new Room { HotelId = hotel.Id, RoomTypeId = suite.Id, RoomNumber = $"3{i:00}", Floor = "3" });
            db.Rooms.AddRange(rooms);
            allRoomsByHotel[hotel.Id] = rooms;
        }

        // ---- GST rule slabs (global, HotelId = null) ---------------------------------------
        var gstEffectiveFrom = new DateOnly(2024, 1, 1);
        db.GstRules.AddRange(
            new GstRule { ChargeCategory = GstChargeCategory.RoomTariff, HsnSac = "996311", MinAmount = 0, MaxAmount = 1000, CgstRate = 0, SgstRate = 0, IgstRate = 0, EffectiveFrom = gstEffectiveFrom },
            new GstRule { ChargeCategory = GstChargeCategory.RoomTariff, HsnSac = "996311", MinAmount = 1000, MaxAmount = 7500, CgstRate = 6, SgstRate = 6, IgstRate = 12, EffectiveFrom = gstEffectiveFrom },
            new GstRule { ChargeCategory = GstChargeCategory.RoomTariff, HsnSac = "996311", MinAmount = 7500, MaxAmount = null, CgstRate = 9, SgstRate = 9, IgstRate = 18, EffectiveFrom = gstEffectiveFrom },
            new GstRule { ChargeCategory = GstChargeCategory.FoodAndBeverage, HsnSac = "996331", MinAmount = 0, MaxAmount = null, CgstRate = 2.5m, SgstRate = 2.5m, IgstRate = 5, EffectiveFrom = gstEffectiveFrom },
            new GstRule { ChargeCategory = GstChargeCategory.OtherServices, HsnSac = "999799", MinAmount = 0, MaxAmount = null, CgstRate = 9, SgstRate = 9, IgstRate = 18, EffectiveFrom = gstEffectiveFrom }
        );

        // ---- Cancellation policy (same demo terms at both hotels) --------------------------
        var cancellationPoliciesByHotel = new Dictionary<Guid, CancellationPolicy>();
        foreach (var hotel in new[] { hotelMumbai, hotelBangalore })
        {
            var policy = new CancellationPolicy { HotelId = hotel.Id, Name = "Standard Flexible Policy" };
            policy.Rules.Add(new CancellationPolicyRule { CancellationPolicyId = policy.Id, HoursBeforeCheckInMin = 168, HoursBeforeCheckInMax = null, PenaltyType = PenaltyType.NoPenalty, SortOrder = 1, Description = "7+ days before check-in: full refund, no penalty." });
            policy.Rules.Add(new CancellationPolicyRule { CancellationPolicyId = policy.Id, HoursBeforeCheckInMin = 24, HoursBeforeCheckInMax = 168, PenaltyType = PenaltyType.OneNightPenalty, SortOrder = 2, Description = "24 hours to 7 days before check-in: one night's room rent is charged as penalty." });
            policy.Rules.Add(new CancellationPolicyRule { CancellationPolicyId = policy.Id, HoursBeforeCheckInMin = null, HoursBeforeCheckInMax = 24, PenaltyType = PenaltyType.FullStayPenalty, AppliesToNoShow = true, SortOrder = 3, Description = "Under 24 hours before check-in, or no-show: full stay value is charged as penalty." });
            db.CancellationPolicies.Add(policy);
            cancellationPoliciesByHotel[hotel.Id] = policy;
        }

        // ---- Rate plans + date-effective price matrix --------------------------------------
        var ratePlanFrom = new DateOnly(2024, 1, 1);
        var ratePlanTo = new DateOnly(2030, 12, 31);
        var ratePlansByHotel = new Dictionary<Guid, List<RatePlan>>();

        foreach (var hotel in new[] { hotelMumbai, hotelBangalore })
        {
            var roomTypes = roomTypesByHotel[hotel.Id];
            var standard = roomTypes[0];
            var deluxe = roomTypes[1];
            var suite = roomTypes[2];

            var roPublic = new RatePlan { HotelId = hotel.Id, Code = "RO-PUB", Name = "Room Only - Public", MealPlan = MealPlanType.RO, Scope = RatePlanScope.Public, CancellationPolicyId = cancellationPoliciesByHotel[hotel.Id].Id };
            var cpPublic = new RatePlan { HotelId = hotel.Id, Code = "CP-PUB", Name = "Bed & Breakfast - Public", MealPlan = MealPlanType.CP, Scope = RatePlanScope.Public, CancellationPolicyId = cancellationPoliciesByHotel[hotel.Id].Id };
            var cpCorporate = new RatePlan { HotelId = hotel.Id, Code = "CP-CORP", Name = "Bed & Breakfast - Corporate", MealPlan = MealPlanType.CP, Scope = RatePlanScope.Corporate, CancellationPolicyId = cancellationPoliciesByHotel[hotel.Id].Id };
            db.RatePlans.AddRange(roPublic, cpPublic, cpCorporate);
            ratePlansByHotel[hotel.Id] = [roPublic, cpPublic, cpCorporate];

            void AddPrices(RatePlan plan, RoomType roomType, decimal occ1Rate, decimal occ2Rate)
            {
                db.RatePlanPrices.Add(new RatePlanPrice { RatePlanId = plan.Id, RoomTypeId = roomType.Id, Occupancy = 1, DayOfWeek = null, EffectiveFrom = ratePlanFrom, EffectiveTo = ratePlanTo, Rate = occ1Rate });
                db.RatePlanPrices.Add(new RatePlanPrice { RatePlanId = plan.Id, RoomTypeId = roomType.Id, Occupancy = 2, DayOfWeek = null, EffectiveFrom = ratePlanFrom, EffectiveTo = ratePlanTo, Rate = occ2Rate });
            }

            AddPrices(roPublic, standard, 3100m, 3500m);
            AddPrices(roPublic, deluxe, 4500m, 5000m);
            AddPrices(roPublic, suite, 7800m, 8500m);

            AddPrices(cpPublic, standard, 3600m, 4000m);
            AddPrices(cpPublic, deluxe, 5000m, 5500m);
            AddPrices(cpPublic, suite, 8300m, 9000m);

            AddPrices(cpCorporate, standard, 3300m, 3700m);
            AddPrices(cpCorporate, deluxe, 4600m, 5100m);
            AddPrices(cpCorporate, suite, 7600m, 8300m);
        }

        // ---- Corporate account + contract (billed from Maharashtra -> same-state at Mumbai) --
        var company = new Company
        {
            Name = "Acme Technologies Pvt Ltd",
            Gstin = "27AACCA1234B1ZQ",
            StateCode = "27",
            BillingAddress = "Level 9, One BKC, Mumbai",
            CreditLimit = 500000m
        };
        db.Companies.Add(company);
        db.CorporateRateContracts.Add(new CorporateRateContract
        {
            CompanyId = company.Id,
            HotelId = hotelMumbai.Id,
            RatePlanId = ratePlansByHotel[hotelMumbai.Id][2].Id, // CP-CORP
            ContractStart = ratePlanFrom,
            ContractEnd = ratePlanTo,
            DiscountPercent = 10m,
            BillToCompanyByDefault = true
        });

        // ---- Travel agent + contract (no live OTA integration - manual contract only) -------
        var travelAgent = new TravelAgent
        {
            Name = "Yatra Global Travels",
            Gstin = "29AAECY1234C1Z8",
            StateCode = "29",
            CommissionPercent = 12m
        };
        db.TravelAgents.Add(travelAgent);
        db.AgentRateContracts.Add(new AgentRateContract
        {
            TravelAgentId = travelAgent.Id,
            HotelId = hotelBangalore.Id,
            RatePlanId = ratePlansByHotel[hotelBangalore.Id][1].Id, // CP-PUB
            ContractStart = ratePlanFrom,
            ContractEnd = ratePlanTo,
            DiscountPercent = 8m
        });

        // ---- POS outlet -----------------------------------------------------------------------
        const string demoPosApiKey = "pos-demo-key-123456";
        var posOutletMumbai = new PosOutlet
        {
            HotelId = hotelMumbai.Id,
            Code = "REST01",
            Name = "Bay View Restaurant",
            ApiKeyHash = HashApiKey(demoPosApiKey),
            DefaultCreditLimit = 25000m
        };
        db.PosOutlets.Add(posOutletMumbai);

        // ---- Demo guests ------------------------------------------------------------------------
        var guestRahul = new Guest { FirstName = "Rahul", LastName = "Sharma", Email = "rahul.sharma@example.com", Phone = "+919820011122", IdProofType = "Aadhaar", IdProofNumber = "XXXX-XXXX-1234", City = "Mumbai", StateCode = "27" };
        var guestPriya = new Guest { FirstName = "Priya", LastName = "Nair", Email = "priya.nair@example.com", Phone = "+919845566778", IdProofType = "Passport", IdProofNumber = "P1234567", City = "Bangalore", StateCode = "29" };
        var guestArjun = new Guest { FirstName = "Arjun", LastName = "Mehta", Email = "arjun.mehta@example.com", Phone = "+919900112233", IdProofType = "DrivingLicence", IdProofNumber = "MH-0212345678", City = "Pune", StateCode = "27" };
        db.Guests.AddRange(guestRahul, guestPriya, guestArjun);

        await db.SaveChangesAsync();

        // ---- Users + roles + hotel access ------------------------------------------------------
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        async Task<ApplicationUser> CreateUserAsync(string userName, string email, string fullName, string role, Guid? hotelId)
        {
            var user = new ApplicationUser { UserName = userName, Email = email, FullName = fullName, EmailConfirmed = true, IsActive = true };
            var result = await userManager.CreateAsync(user, DemoPassword);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to seed user '{userName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            await userManager.AddToRoleAsync(user, role);

            if (hotelId is not null)
            {
                db.UserHotelAccesses.Add(new UserHotelAccess { UserId = user.Id, HotelId = hotelId.Value });
            }

            return user;
        }

        await CreateUserAsync("superadmin", "superadmin@stayops.in", "Ananya Krishnan (Super Admin)", Roles.SuperAdmin, null);
        await CreateUserAsync("manager.mumbai", "manager.mumbai@stayops.in", "Vikram Desai (Mumbai GM)", Roles.HotelManager, hotelMumbai.Id);
        await CreateUserAsync("manager.bangalore", "manager.bangalore@stayops.in", "Sneha Rao (Bangalore GM)", Roles.HotelManager, hotelBangalore.Id);
        await CreateUserAsync("reception.mumbai", "reception.mumbai@stayops.in", "Karan Malhotra (Front Desk)", Roles.Receptionist, hotelMumbai.Id);
        await CreateUserAsync("finance.mumbai", "finance.mumbai@stayops.in", "Divya Iyer (Finance)", Roles.FinanceUser, hotelMumbai.Id);
        await CreateUserAsync("housekeeping.mumbai", "housekeeping.mumbai@stayops.in", "Suresh Pillai (Housekeeping)", Roles.Housekeeper, hotelMumbai.Id);
        await CreateUserAsync("pos.mumbai", "pos.mumbai@stayops.in", "Bay View Restaurant POS", Roles.POSSystem, hotelMumbai.Id);

        await db.SaveChangesAsync();

        // ---- Sample stays + finance records (via the real stored procedures, not raw INSERTs) ----
        // Requires database/04-stored-procedures scripts to already be applied (see scripts/setup-database.ps1).
        // Best-effort: logged and skipped (not fatal to API startup) if the procs aren't present yet.
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("StayOps.DemoDataSeeder");
        try
        {
            await SampleStaySeeder.SeedAsync(serviceProvider, hotelMumbai.Id, roomTypesByHotel[hotelMumbai.Id], ratePlansByHotel[hotelMumbai.Id],
                guestRahul.Id, guestArjun.Id, company.Id, posOutletMumbai.Code, demoPosApiKey);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Skipped sample stay/finance seeding - run scripts/setup-database.ps1 to apply stored procedures, then restart the API.");
        }
    }

    private static string HashApiKey(string rawKey)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawKey));
        return Convert.ToHexString(bytes);
    }
}
