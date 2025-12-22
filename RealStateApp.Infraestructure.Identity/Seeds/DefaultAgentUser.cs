
using Microsoft.AspNetCore.Identity;
using RealStateApp.Core.Domain.Common.Enums;
using RealStateApp.Infraestructure.Identity.Entities;

namespace RealStateApp.Infraestructure.Identity.Seeds
{
    public class DefaultAgentUser
    {
        public static async Task SeedAsync(UserManager<AppUser> userManager)
        {
            var defaultUser = new AppUser
            {
                UserName = "agent",
                Email = "agent@app.com",
                FirstName = "Default",
                LastName = "Agent",
                EmailConfirmed = true,
                IsActive = true
            };

            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, "123Pa$$word!");
                await userManager.AddToRoleAsync(defaultUser, AppRoles.AGENT.ToString());
            }




            var agents = new List<AppUser>
    {
        new AppUser
        {
            UserName = "agent1",
            Email = "agent1@app.com",
            FirstName = "Agente",
            LastName = "Uno",
            EmailConfirmed = true,
            IsActive = true,
            Photo = "https://i.pinimg.com/originals/33/2b/c7/332bc749fcb06a540e08bca788301e71.jpg"
        },
        new AppUser
        {
            UserName = "agent2",
            Email = "agent2@app.com",
            FirstName = "Agente",
            LastName = "Dos",
            EmailConfirmed = true,
            IsActive = true,
            Photo = "https://i.pinimg.com/originals/33/2b/c7/332bc749fcb06a540e08bca788301e71.jpg"
        },
        new AppUser
        {
            UserName = "agent3",
            Email = "agent3@app.com",
            FirstName = "Agente",
            LastName = "Tres",
            EmailConfirmed = true,
            IsActive = true,
            Photo = "https://i.pinimg.com/originals/33/2b/c7/332bc749fcb06a540e08bca788301e71.jpg"
        }
    };

            foreach (var agent in agents)
            {
                var _user = await userManager.FindByEmailAsync(agent.Email!);
                if (_user == null)
                {
                    await userManager.CreateAsync(agent, "Agent123!");
                  
                    await userManager.AddToRoleAsync(agent, AppRoles.AGENT.ToString());
                }
            }

        }
    }
}
