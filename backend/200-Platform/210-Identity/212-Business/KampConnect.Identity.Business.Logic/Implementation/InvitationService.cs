using System;
using System.Collections.Generic;
using System.Text;
using KampConnect.Identity.Business.Contracts.ClassTypes;
using KampConnect.Identity.Business.Contracts.Interfaces;
using KampConnect.Identity.Business.Contracts.Requests;
using KampConnect.Identity.Data.Entity.Tenant.Invitations;
using KampConnect.Identity.Data.Interface.Repository;

namespace KampConnect.Identity.Business.Logic.Implementation;

public class InvitationService(ITenantInvitationRepository tenantInvitationRepository) : IInvitationService
{
    public Task<InvitationResult> CreateInvitationAsync(CreateInvitationRequest request, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var Invitation = new TenantInvitation
        {
            tenant_id = request.TenantId,
            email = request.Email,
            role_id = request.RoleId,
            sent_by_user_id = request.SentByUserId,
        }
    }
}
