using System.ComponentModel.DataAnnotations;

namespace TMS201.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; } // Employee, Admin, SuperAdmin, TeamLead
    }
}
