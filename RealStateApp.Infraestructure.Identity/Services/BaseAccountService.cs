using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using RealStateApp.Core.Application.Dtos.Email;
using RealStateApp.Core.Application.Dtos.User;
using RealStateApp.Core.Application.Interfaces;
using RealStateApp.Infraestructure.Identity.Entities;
using System.Text;

namespace RealStateApp.Infraestructure.Identity.Services
{
    public class BaseAccountService : IBaseAccountService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;

        public BaseAccountService(UserManager<AppUser> userManager, IEmailService emailService, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _emailService = emailService;
        }

        public async Task<RegisterUserResponseDto> RegisterAsync(SaveUserDto dto, string? origin)
        {
            RegisterUserResponseDto response = new()
            {
                HasError = false,
                Errors = new List<string>(),
                UserName = string.Empty,
                Email = string.Empty,
                FirstName = string.Empty,
                LastName = string.Empty
            };

            try
            {
                var emailExists = await _userManager.FindByEmailAsync(dto.Email);
                if (emailExists != null)
                {
                    response.HasError = true;
                    response.Errors.Add($"Este correo ({dto.Email}) ya está en uso.");
                    return response;
                }

                var userNameExists = await _userManager.FindByNameAsync(dto.UserName);
                if (userNameExists != null)
                {
                    response.HasError = true;
                    response.Errors.Add($"Este nombre de usuario ({dto.UserName}) ya está en uso.");
                    return response;
                }

                AppUser user = new()
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    UserName = dto.UserName,
                    EmailConfirmed = false,
                    Dni = dto.Dni,
                    PhoneNumber = dto.Phone,
                    Photo = dto.Photo
                };

                var dtoRole = dto.Roles?.First();
                if (dtoRole == null)
                {
                    response.HasError = true;
                    response.Errors.Add("El rol es requerido.");
                    return response;
                }

                switch (dtoRole.ToLower())
                {
                    case "client":
                        user.IsActive = false;
                        user.EmailConfirmed = false;
                        break;

                    case "agent":
                        user.IsActive = false;
                        user.EmailConfirmed = true;
                        break;

                    case "admin":
                    case "developer":
                        user.IsActive = true;
                        user.EmailConfirmed = true;
                        break;
                }

                var result = await _userManager.CreateAsync(user, dto.Password);

                if (!result.Succeeded)
                {
                    response.HasError = true;
                    response.Errors.AddRange(result.Errors.Select(err => err.Description));
                    return response;
                }

                await _userManager.AddToRoleAsync(user, dtoRole.ToUpper());

                if (dtoRole.ToLower() == "client" && origin != null)
                {
                    string confirmationUrl = await GetVerificationEmailUri(user, origin);
                    string emailHtml = GetEmailTemplate(user.FirstName, confirmationUrl);

                    await _emailService.SendAsync(new EmailRequestDto
                    {
                        To = dto.Email,
                        Subject = "Confirma tu cuenta – RealStateApp",
                        BodyHtml = emailHtml
                    });
                }

                // Manual mapping from AppUser to RegisterUserResponseDto (Clean Architecture)
                response.Id = user.Id;
                response.Email = user.Email ?? string.Empty;
                response.UserName = user.UserName ?? string.Empty;
                response.FirstName = user.FirstName;
                response.LastName = user.LastName;
                response.Dni = user.Dni;
                response.IsVerified = user.EmailConfirmed;
                response.Roles = new List<string> { dtoRole.ToUpper() };
                
                return response;
            }
            catch (Exception ex)
            {
                response.HasError = true;
                response.Errors ??= new List<string>();
                response.Errors.Add($"Error interno: {ex.Message}");
                return response;
            }
        }
        public virtual async Task<EditUserResponseDto> EditUser(SaveUserDto saveDto, bool? isCreated = false)
        {
            bool isNotCreated = !(isCreated ?? false);

            EditUserResponseDto response = new()
            {
                Email = "",
                Id = "",
                LastName = "",
                Name = "",
                UserName = "",
                HasError = false,
                IsVerified = true,
                Errors = []
            };

            // Obtener el usuario que se está editando
            var user = await _userManager.FindByIdAsync(saveDto.Id ?? "");

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add("No account was found with this user ID.");
                return response;
            }

            // Guardar el ID real del usuario para evitar problemas si saveDto.Id es null o vacío
            string currentUserId = user.Id;

            // Validación de username duplicado (asegurar que no sea él mismo)
            var userWithSameUserName = await _userManager.Users
                .FirstOrDefaultAsync(w => w.UserName == saveDto.UserName && w.Id != currentUserId);

            if (userWithSameUserName != null)
            {
                response.HasError = true;
                response.Errors.Add($"El nombre de usuario '{saveDto.UserName}' ya está asociado a otra cuenta");
                return response;
            }

            // Validación de email duplicado (evitar comparar con sí mismo)
            var userWithSameEmail = await _userManager.Users
                .FirstOrDefaultAsync(w => w.Email == saveDto.Email && w.Id != currentUserId);

            if (userWithSameEmail != null)
            {
                response.HasError = true;
                response.Errors.Add($"El correo '{saveDto.Email}' ya está asociado a otra cuenta");
                return response;
            }

            // Actualizar datos
            user.FirstName = saveDto.FirstName;
            user.LastName = saveDto.LastName;
            user.UserName = saveDto.UserName;
            user.Email = saveDto.Email;
            user.Dni = saveDto.Dni;

            if (saveDto.Photo != null)
                user.Photo = saveDto.Photo;

            // Cambiar contraseña solo si no es creación
            if (!string.IsNullOrWhiteSpace(saveDto.Password) && isNotCreated)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resultChange = await _userManager.ResetPasswordAsync(user, token, saveDto.Password);

                if (!resultChange.Succeeded)
                {
                    response.HasError = true;
                    response.Errors.AddRange(resultChange.Errors.Select(s => s.Description));
                    return response;
                }
            }

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                response.HasError = true;
                response.Errors.AddRange(result.Errors.Select(s => s.Description));
                return response;
            }

            // Respuesta final
            response.Id = user.Id;
            response.Email = user.Email ?? "";
            response.UserName = user.UserName ?? "";
            response.Name = user.FirstName;
            response.LastName = user.LastName;
            response.IsVerified = user.EmailConfirmed;

            return response;
        }

        public virtual async Task<UserResponseDto> DeleteAsync(string id)
        {
            UserResponseDto response = new() { HasError = false, Errors = new List<string>() };

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add("No existe un usuario con este ID.");
                return response;
            }

            await _userManager.DeleteAsync(user);
            return response;
        }

        protected async Task<string> GetVerificationEmailUri(AppUser user, string origin)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var route = "Login/ConfirmEmail";
            var completeUrl = new Uri($"{origin}/{route}");

            var verificationUri = QueryHelpers.AddQueryString(completeUrl.ToString(), "userId", user.Id);
            verificationUri = QueryHelpers.AddQueryString(verificationUri, "token", encodedToken);

            return verificationUri;
        }

        protected async Task<string> GetResetPasswordUri(AppUser user, string origin)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var route = "Login/ResetPassword";
            var completeUrl = new Uri($"{origin}/{route}");

            var resetUri = QueryHelpers.AddQueryString(completeUrl.ToString(), "userId", user.Id);
            resetUri = QueryHelpers.AddQueryString(resetUri, "token", encodedToken);

            return resetUri;
        }

        private string GetEmailTemplate(string name, string url)
        {
            return $@"
<body style=""font-family: Arial, sans-serif; background-color: #f7f7f7; padding: 20px;"">
    <div style=""max-width: 600px; margin: auto; background-color: white; padding: 30px; border-radius: 8px; border: 1px solid #e0e0e0;"">
        
        <h2 style=""color: #2b4c7e; text-align: center; margin-top: 0;"">
            Confirmación de Cuenta – RealStateApp
        </h2>

        <p style=""font-size: 15px; color: #444;"">
            Estimado/a <strong>{name}</strong>,
        </p>

        <p style=""font-size: 15px; color: #555; line-height: 1.6;"">
            Gracias por registrarse en nuestra plataforma inmobiliaria.  
            Para activar su cuenta y acceder a nuestros servicios, confirme su correo electrónico.
        </p>

        <div style=""text-align: center; margin: 25px 0;"">
            <a href=""{url}""
               style=""background-color: #2b4c7e; color: white; text-decoration: none; padding: 12px 25px; border-radius: 5px; font-size: 15px;"">
                Confirmar Cuenta
            </a>
        </div>

        <p style=""font-size: 15px; color: #555; line-height: 1.6;"">
            Si usted no solicitó esta cuenta, puede ignorar este mensaje.
        </p>

        <hr style=""border: none; height: 1px; background-color: #ddd; margin: 30px 0;"">

        <p style=""font-size: 13px; color: #888; text-align: center; line-height: 1.4;"">
            © 2025 RealStateApp. Todos los derechos reservados.<br>
            Este es un mensaje automático, por favor no responder.
        </p>

    </div>
</body>";
        }
    }
}
