using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectsManager.Database
{
    public class Project
    {
        public static readonly string IN_PROGRESS = "IN_PROGRESS";
        public static readonly string FINISHED = "FINISHED";
        public static readonly string[] ProjectStatus = new string[] { IN_PROGRESS, FINISHED };

        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDateTime { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDateTime { get; set; }
        public string Status { get; set; }


        public List<ProjectTask> ProjectTasks { get; set; }

    }
}
