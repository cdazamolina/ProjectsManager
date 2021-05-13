using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProjectsManager.Database
{
    public class User : IdentityUser
    {
        public bool IsEnable { get; set; }
    }
}
