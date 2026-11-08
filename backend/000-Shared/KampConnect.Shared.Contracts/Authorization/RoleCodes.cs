using System;
using System.Collections.Generic;
using System.Text;

namespace KampConnect.Shared.Contracts.Authorization;

public static class RoleCodes
{
    public const string Guardian = "guardian";
    public const string TenantAdmin = "tenant_admin";
    public const string PlatformAdmin = "platform_admin";
    public const string Staff = "staff";
    public const string Participant = "participant";
}
