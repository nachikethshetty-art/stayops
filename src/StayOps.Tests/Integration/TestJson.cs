using System.Text.Json;
using System.Text.Json.Serialization;

namespace StayOps.Tests.Integration;

/// <summary>
/// The API serializes enums as strings (see Program.cs JsonStringEnumConverter registration).
/// HttpClient's GetFromJsonAsync/PostAsJsonAsync extension methods use default System.Text.Json
/// options unless told otherwise, so tests must pass this explicitly to deserialize enum properties.
/// </summary>
public static class TestJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
}
