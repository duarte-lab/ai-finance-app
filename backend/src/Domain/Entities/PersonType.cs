using System.Text.Json.Serialization;

namespace Domain.Entities;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PersonType
{
    Owner = 1,
    Guest = 2,
}
