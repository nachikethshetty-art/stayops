using FluentAssertions;
using StayOps.Application.Billing;
using StayOps.Application.Hotels;
using StayOps.Domain.Enums;

namespace StayOps.Tests.Unit;

public class ValidatorTests
{
    [Fact]
    public void CreateHotelRequestValidator_RejectsNonTwoDigitStateCode()
    {
        var validator = new CreateHotelRequestValidator();
        var request = new CreateHotelRequest(
            Guid.NewGuid(), "TST01", "Test Hotel", "Addr", "", "City", "400001",
            StateCode: "MH", "Maharashtra", "27AAAAA0000A1Z5", "Asia/Kolkata");

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateHotelRequest.StateCode));
    }

    [Fact]
    public void CreateHotelRequestValidator_AcceptsValidTwoDigitStateCodeAndGstin()
    {
        var validator = new CreateHotelRequestValidator();
        var request = new CreateHotelRequest(
            Guid.NewGuid(), "TST01", "Test Hotel", "Addr", "", "City", "400001",
            StateCode: "27", "Maharashtra", "27AAAAA0000A1Z5", "Asia/Kolkata");

        var result = validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void PostChargeRequestValidator_RejectsZeroOrNegativeAmount()
    {
        var validator = new PostChargeRequestValidator();
        var request = new PostChargeRequest(FolioTransactionType.Incidental, GstChargeCategory.FoodAndBeverage, "Snacks", 0);

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PostChargeRequest.TaxableAmount));
    }

    [Fact]
    public void PostChargeRequestValidator_RejectsRoomTaxChargeType()
    {
        var validator = new PostChargeRequestValidator();
        var request = new PostChargeRequest(FolioTransactionType.Payment, GstChargeCategory.RoomTariff, "Should not be allowed", 100);

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }
}
