using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectsManager.Core.Users.Update
{
    public class ToggleUserStatus
    {
        [Required]
        public bool IsEnable { get; set; }
    }
}
