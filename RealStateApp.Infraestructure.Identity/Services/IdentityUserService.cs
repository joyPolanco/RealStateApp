using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Bcpg.OpenPgp;
using RealStateApp.Core.Application.Dtos.User;
using RealStateApp.Core.Application.Helpers;
using RealStateApp.Core.Application.Interfaces;
using RealStateApp.Core.Domain.Common.Enums;
using RealStateApp.Infraestructure.Identity.Contexts;
using RealStateApp.Infraestructure.Identity.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealStateApp.Infraestructure.Identity.Services
{
    public class IdentityUserService : IUserService
    {
        private UserManager<AppUser> _userManager;
        private readonly IdentityContext _identityContext;
        
        public IdentityUserService(UserManager<AppUser> userManager, IdentityContext identityDbContext, IHttpContextAccessor context)
        {
            _userManager = userManager;
            _identityContext = identityDbContext;
        }

        public virtual async Task<UserResponseDto> DeleteAsync(string id)
        {
            UserResponseDto response = new() { HasError = false, Errors = [] };

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
        public virtual async Task ToogleState(string id)
        {

            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                user.IsActive = !user.IsActive;
                await _userManager.UpdateAsync(user);

            }


        }
        public virtual async Task<bool> SetStatus(string id, bool status)
        {

            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                user.IsActive = status;
                await _userManager.UpdateAsync(user);
                return true;
            }

            return false;
        }

        public async Task<UserDto?> GetByDni(string dni)
        {
            var cleanDocumentId = dni?.Trim().Replace("-", "").Replace(" ", "") ?? "";

            var user = await _userManager.Users
                .Where(r => r.Dni != null && r.Dni.Replace("-", "").Replace(" ", "") == cleanDocumentId)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return null;
            }

            var rolesList = await _userManager.GetRolesAsync(user!);
            var role = EnumMapper<AppRoles>.FromString(rolesList.First()!);

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

        public async Task<IList<AgentDto>> GetUsersAgentOnly(Dictionary<string, int> dictionary)
        {
            var agents = await _userManager.GetUsersInRoleAsync(AppRoles.AGENT.ToString());

            return agents
                .Select(user => new AgentDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsActive = user.IsActive,
                    IsVerified = user.EmailConfirmed,
                    PropertiesCount = dictionary.GetValueOrDefault(user.Id, 0)
                })
                .ToList();
        }


        public async Task<List<UserDto>> GetUsersByRole(string role, string displayRole)
        {

            var users = await _userManager.GetUsersInRoleAsync(role);

            return users
                .Select(user => new UserDto
                {
                    Id = user.Id,
                    Dni = user.Dni,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsActive = user.IsActive,
                    UserName = user.UserName!,
                    Role = displayRole,
                    Photo = user.Photo!,    
                    IsVerified = user.EmailConfirmed
                })
                .ToList();
        }

        public async Task<List<UserDto>> GetUsersAdminOnly()
        {
            return await GetUsersByRole("ADMIN", EnumMapper<AppRoles>.ToString(AppRoles.ADMIN));

        }

        public async Task<List<UserDto>> GetUsersDevelopersOnly()
        {
            return await GetUsersByRole("DEVELOPER", EnumMapper<AppRoles>.ToString(AppRoles.DEVELOPER));

        }

        public async Task<List<UserDto>> GetClientsDevelopersOnly()
        {
            return await GetUsersByRole("CLIENT", EnumMapper<AppRoles>.ToString(AppRoles.CLIENT));

        }


        private async Task<int> GetActiveByRoleCount(string role)
        {
            var users = await _userManager.GetUsersInRoleAsync(role);

            return users.Where(r => r.IsActive).Count();
        }

        private async Task<int> GetInactiveByRoleCount(string role)
        {
            var users = await _userManager.GetUsersInRoleAsync(role);

            return users.Where(r => !r.IsActive).Count();
        }

        public async Task<int> GetActiveClientsCount()
        {
            return await GetActiveByRoleCount(AppRoles.CLIENT.ToString());

        }
        public async Task<int> GetInactiveClientsCount()
        {
            return await GetInactiveByRoleCount(AppRoles.CLIENT.ToString());

        }

        public async Task<int> GetInactiveAgentsCount()
        {
            return await GetInactiveByRoleCount(AppRoles.AGENT.ToString());

        }
        public async Task<int> GetActiveAgentsCount()
        {
            return await GetActiveByRoleCount(AppRoles.AGENT.ToString());

        }

        public async Task<int> GetActiveDevelopersCount()
        {
            return await GetActiveByRoleCount(AppRoles.DEVELOPER.ToString());

        }
        public async Task<int> GetInactiveDevelopersCount()
        {
            return await GetInactiveByRoleCount(AppRoles.DEVELOPER.ToString());

        }
        public async Task<UserDto?> GetById(string Id)
        {

            var user = await _userManager.Users
                .Where(r => r.Id== Id)
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



        public async Task<List<UserDto>> GetUsersAgentnOnly()
        {
            return await GetUsersByRole("AGENT", EnumMapper<AppRoles>.ToString(AppRoles.AGENT));
        }

       
    }

}
