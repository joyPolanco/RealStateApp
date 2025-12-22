using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealStateApp.Core.Application.ViewModels.User
{
    public class UserViewModel
    {
        public required string Id { get; set; }

        public required string UserName { get; set; }

        public required string Email { get; set; }
        public bool IsVerified { get; set; }

        public required string FirstName { get; set; }

        public required string LastName { get; set; }

        public string? Dni { get; set; }

        public required string Role { get; set; }

        public bool IsActive { get; set; }
    }
}
