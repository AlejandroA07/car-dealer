namespace WestcoastCars.Auth.Contracts.Admin
{
    public record CreateUserRequest(
        string FirstName,
        string LastName,
        string Email,
        string Password,
        string Role
    );
}
