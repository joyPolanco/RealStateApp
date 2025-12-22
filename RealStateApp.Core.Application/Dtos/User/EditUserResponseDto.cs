namespace RealStateApp.Core.Application.Dtos.User
{
    public class EditUserResponseDto
    {
        public required string  Email { get; set; }
        public required string Id { get; set; }
        public required string LastName { get; set; }
        public required string Name { get; set; }
        public required string UserName { get; set; }
        public bool HasError { get; set; }
        public string? Dni { get; set; }
        public bool IsVerified { get; set; }
        public List<string>? Errors { get; set; }
    }
}