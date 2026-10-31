namespace KampConnect.User.Business.Contracts.Requests;

public record UserCreate(string FirstName, string LastName, string BirthDate, string PreferredLanguage);
