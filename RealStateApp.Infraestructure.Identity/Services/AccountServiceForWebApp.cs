using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using RealStateApp.Core.Application.Dtos.Login;
using RealStateApp.Core.Application.Dtos.User;
using RealStateApp.Core.Application.Helpers;
using RealStateApp.Core.Application.Interfaces;
using RealStateApp.Core.Domain.Common.Enums;
using RealStateApp.Infraestructure.Identity.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealStateApp.Infraestructure.Identity.Services
{
    public class AccountServiceForWebApp : BaseAccountService, IAccountServiceForWebApp
    {

        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailService _emailService;

        public AccountServiceForWebApp(UserManager<AppUser> userManager, IEmailService emailService, SignInManager<AppUser> signInManager) : base(userManager, emailService, signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
        }
        public virtual async Task<UserResponseDto> ConfirmAccountAsync(string token, string? userId = null)
        {
            UserResponseDto response = new()
            {
                HasError = false,
                Errors = new List<string>()
            };

            if (string.IsNullOrEmpty(userId))
            {
                response.Message = "No se proporcionó un UserId válido";
                response.HasError = true;
                return response;
            }

            try
            {
                token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            }
            catch
            {
                response.Message = "El token proporcionado es inválido o está corrupto";
                response.HasError = true;
                return response;
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                response.Message = "No existe ninguna cuenta asociada a este usuario";
                response.HasError = true;
                return response;
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                user.IsActive = true;
                await _userManager.UpdateAsync(user);

                response.Message = $"Cuenta confirmada correctamente para {user.Email}. Ya puedes iniciar sesión.";
                return response;
            }

            response.Message = $"Hubo un error al confirmar la cuenta de {user.Email}.";
            response.HasError = true;

            return response;
        }
        public async Task SignOutAsync()
        {

            await _signInManager.SignOutAsync();
        }

        public async Task<LoginResponseDto> AuthenticateAsync(LoginDto loginDto)
        {

            LoginResponseDto responseDto = new LoginResponseDto() 
            { 
                HasError = false,
                Id = string.Empty,
                Email = string.Empty,
                UserName = string.Empty
            };
            
            // Intentar buscar por nombre de usuario primero, luego por email
            var user = await _userManager.FindByNameAsync(loginDto.Username);
            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(loginDto.Username);
            }
            
            if (user == null)
            {
                responseDto.HasError = true;
                responseDto.Error = $"No hay ningún usuario con el nombre de usuario o correo {loginDto.Username}";
                return responseDto;
            }

            if (!user.EmailConfirmed)
            {
                responseDto.HasError = true;
                responseDto.Error = $"Esta cuenta no está activa. Actívala mediante el link que ha sido enviado a tu correo";
                return responseDto;


            }

            if (!user.IsActive)
            {
                responseDto.HasError = true;
                responseDto.Error = $"Esta cuenta está desactivada. Contacta al administrador para más información";
                return responseDto;
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName ?? "", loginDto.Password, false, false);
            if (!result.Succeeded)
            {
                responseDto.HasError = true;
                responseDto.Error = $"Usuario o contraseña incorrectos";
                return responseDto;

            }
            
            // Manual mapping from AppUser to LoginResponseDto (Clean Architecture)
            responseDto.Id = user.Id;
            responseDto.Email = user.Email ?? string.Empty;
            responseDto.UserName = user.UserName ?? string.Empty;
            responseDto.IsVerified = user.EmailConfirmed && user.IsActive;
            var rolesList = await _userManager.GetRolesAsync(user);
            responseDto.Roles = rolesList.ToList();


            return responseDto;
        }



        public async Task<UserDto?> GetByUserName(string name)
        {

            var user = await _userManager.Users
                .Where(r => r.UserName!.Replace("-", "").Replace(" ", "") == name)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return null;
            }

            var rolesList = await _userManager.GetRolesAsync(user);
            var role = EnumMapper<AppRoles>.FromString(rolesList.First());

            var userDto = new UserDto()
            {
                Id = user.Id,
                Email = user.Email ?? "",
                LastName = user.LastName,
                FirstName = user.FirstName,
                UserName = user.UserName ?? "",
                Dni = user.Dni,
                IsVerified = user.EmailConfirmed,
                IsActive = user.IsActive,
                Role = EnumMapper<AppRoles>.ToString(role)
            };

            return userDto;
        }


        public async Task<List<UserDto>> GetByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return new List<UserDto>();

         
            var normalizedSearch = name.Replace("-", "").Replace(" ", "").ToLower();

            var users = await _userManager.Users
                .Where(u =>
                    (u.FirstName + " " + u.LastName).Replace("-", "").Replace(" ", "").ToLower().Contains(normalizedSearch)
                    || u.FirstName.Replace("-", "").Replace(" ", "").ToLower().Contains(normalizedSearch)
                    || u.LastName.Replace("-", "").Replace(" ", "").ToLower().Contains(normalizedSearch) && u.IsActive == true
                )
                .ToListAsync();

            if (!users.Any())
                return new List<UserDto>();

            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var rolesList = await _userManager.GetRolesAsync(user);
                var role = rolesList.Any() ? EnumMapper<AppRoles>.FromString(rolesList.First()) : AppRoles.AGENT;

                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    LastName = user.LastName,
                    FirstName = user.FirstName,
                    UserName = user.UserName ?? "",
                    Dni = user.Dni,
                    IsVerified = user.EmailConfirmed,
                    IsActive = user.IsActive,
                    Photo = user.Photo,
                    Role = EnumMapper<AppRoles>.ToString(role)
                });
            }

            return userDtos;
        }



        public async Task<UserDto?> GetById(string id)
        {

            var user = await _userManager.Users
                .Where(r => r.Id == id)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return null;
            }

            var rolesList = await _userManager.GetRolesAsync(user);
            var role = EnumMapper<AppRoles>.FromString(rolesList.First());

            var userDto = new UserDto()
            {
                Id = user.Id,
                Email = user.Email ?? "",
                LastName = user.LastName,
                FirstName = user.FirstName,
                UserName = user.UserName ?? "",
                Dni = user.Dni,
                IsVerified = user.EmailConfirmed,
                IsActive = user.IsActive,
                Photo = user.Photo ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                Role = EnumMapper<AppRoles>.ToString(role)
            };

            return userDto;
        }
    



        public async Task<UserResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto dto)
        {
            UserResponseDto response = new()
            {
                HasError = false,
                Errors = new List<string>()
            };

            var user = await _userManager.FindByNameAsync(dto.Username);
            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"No existe ningún usuario con el nombre de usuario {dto.Username}");
                return response;
            }

            if (!user.EmailConfirmed || !user.IsActive)
            {
                response.HasError = true;
                response.Errors.Add("Esta cuenta no está activa o no ha sido confirmada");
                return response;
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken));

            var resetUrl = $"{dto.Origin}/Login/ResetPassword?userId={user.Id}&token={encodedToken}";

            await _emailService.SendAsync(new Core.Application.Dtos.Email.EmailRequestDto
            {
                To = user.Email,
                Subject = "Restablecer Contraseña - Real State App",
                BodyHtml = $@"
                    <h2>Restablecer Contraseña</h2>
                    <p>Hola {user.FirstName},</p>
                    <p>Has solicitado restablecer tu contraseña. Haz clic en el siguiente enlace para continuar:</p>
                    <p><a href='{resetUrl}' style='background-color: #667eea; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block;'>Restablecer Contraseña</a></p>
                    <p>Si no solicitaste este cambio, puedes ignorar este correo.</p>
                    <p>Este enlace expirará en 12 horas.</p>
                    <br/>
                    <p>Saludos,<br/>El equipo de Real State App</p>"
            });

            response.Message = "Se ha enviado un correo con las instrucciones para restablecer tu contraseña";
            return response;
        }

        public async Task<UserResponseDto> ResetPasswordAsync(ResetPasswordRequestDto dto)
        {
            UserResponseDto response = new()
            {
                HasError = false,
                Errors = new List<string>()
            };

            var user = await _userManager.FindByIdAsync(dto.Id);
            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add("No existe ningún usuario asociado a esta solicitud");
                return response;
            }

            try
            {
                var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Token));
                var result = await _userManager.ResetPasswordAsync(user, decodedToken, dto.Password);

                if (result.Succeeded)
                {
                    response.Message = "Tu contraseña ha sido restablecida exitosamente";
                    return response;
                }

                response.HasError = true;
                foreach (var error in result.Errors)
                {
                    response.Errors.Add(error.Description);
                }
                return response;
            }
            catch
            {
                response.HasError = true;
                response.Errors.Add("El token de restablecimiento es inválido o ha expirado");
                return response;
            }
        }

      
    }
}
