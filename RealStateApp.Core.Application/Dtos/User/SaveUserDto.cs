namespace RealStateApp.Core.Application.Dtos.User
{
    public class SaveUserDto
    {
        public string? Id { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string Dni { get; set; } = string.Empty;
        public string? Photo { get; set; }
        public string? Phone { get; set; }
        public string? ConfirmPassword { get; set; }

        public List<string> Roles { get; set; } = new List<string>();

    }
}
