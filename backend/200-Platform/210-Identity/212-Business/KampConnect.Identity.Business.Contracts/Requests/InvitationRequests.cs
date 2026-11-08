 namespace KampConnect.Identity.Business.Contracts.Requests;

 /// <summary>Nieuwe uitnodiging: e-mailadres + rol die de persoon binnen de tenant krijgt.</summary>
 public record CreateInvitationRequest(string Email, int RoleId);

 /// <summary>Uitnodiging aanvaarden door een ingelogde (bestaande) gebruiker.</summary>
 public record AcceptInvitationRequest(string Token);

 /// <summary>Uitnodiging weigeren (het token uit de mail volstaat, inloggen hoeft niet).</summary>
 public record DeclineInvitationRequest(string Token);
