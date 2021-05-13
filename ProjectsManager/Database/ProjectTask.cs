using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectsManager.Database
{
    public class ProjectTask
    {
        public static readonly string IN_PROGRESS = "IN_PROGRESS";
        public static readonly string FINISHED = "FINISHED";
        public static readonly string[] ProjectTaskStatus = new string[] { IN_PROGRESS, FINISHED };

        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime ExecutionDateTime { get; set; }
        public string Status { get; set; }


        public int ProjectId { get; set; }
        public Project Project { get; set; }
    }
}
