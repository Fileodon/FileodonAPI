using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Infrastructure.Models.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SFTPConnectionStatus
    {
        Accepted,
        Rejected,
        Pending
    }
}
