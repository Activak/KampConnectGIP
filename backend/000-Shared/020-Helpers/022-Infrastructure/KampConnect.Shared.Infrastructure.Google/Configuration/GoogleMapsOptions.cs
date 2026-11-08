using System;
using System.Collections.Generic;
using System.Text;

namespace KampConnect.Shared.Infrastructure.Google.Configuration;

internal sealed class GoogleMapsOptions
{
    public const string SectionName = "GoogleMaps";

    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://addressvalidation.googleapis.com/";
}
