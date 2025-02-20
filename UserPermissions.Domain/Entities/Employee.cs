using System.Collections.Generic;

namespace UserPermissions.Domain.Entities
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Initialize with default value
        public string Email { get; set; } = string.Empty; // Initialize with default value
        public ICollection<Permission> Permissions { get; set; } = new List<Permission>(); // Initialize with default value
    }
}