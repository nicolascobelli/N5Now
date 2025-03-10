namespace UserPermissions.Application.DTOs
{
    public class PermissionDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
