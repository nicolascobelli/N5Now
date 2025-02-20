using System.Collections.Generic;

namespace UserPermissions.Domain.Entities
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public List<Permission> Permissions { get; set; }
    }
}