using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectsManager.Database
{
    public class Roles : IdentityRole
    {
        public static readonly string ADMINISTRATOR = "Administrator";
        public static readonly string OPERATOR = "Operator";
    }
}
