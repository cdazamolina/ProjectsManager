using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectsManager.Core.Projects.Update
{
    public class ProjectUpdateRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDateTime { get; set; }
    }
}
