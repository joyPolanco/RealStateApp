using Newtonsoft.Json;

namespace RealStateApp.Core.Application.Dtos.User
{
    public class UserDto
    {
        public required string Id { get; set; }
        [JsonProperty("usuario")]

        public required string UserName { get; set; }
        [JsonProperty("correo")]

        public required string Email { get; set; }
        [JsonIgnore]
        public bool IsVerified { get; set; }
        [JsonProperty("nombre")]

        public required string FirstName { get; set; }
        [JsonProperty("apellido")]

        public required string LastName { get; set; }
        [JsonProperty("cedula")]

        public string? Dni { get; set; }
        [JsonProperty("rol")]

        public required string Role { get; set; }

        public string? Photo {  get; set; }
        public string? PhoneNumber {  get; set; }
        public bool IsActive { get; set; }
        





    }
}
