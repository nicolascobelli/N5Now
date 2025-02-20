namespace UserPermissions.Domain.Entities
{
    public class Permission
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
        public int PermissionTypeId { get; set; }
        public PermissionType PermissionType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public Permission(string description, int employeeId, int permissionTypeId, DateTime startDate, DateTime endDate)
        {
            ValidateValidDates(startDate, endDate);

            Description = description;
            EmployeeId = employeeId;
            PermissionTypeId = permissionTypeId;
            StartDate = startDate;
            EndDate = endDate;
        }

        public void Update(DateTime startDate, DateTime endDate)
        {
            ValidateValidDates(startDate, endDate);

            StartDate = startDate;
            EndDate = endDate;
        }

        private void ValidateValidDates(DateTime startDate, DateTime endDate)
        {
            if (endDate <= startDate)
            {
                throw new ArgumentException("End date must be greater than start date.");
            }

            if (startDate <= DateTime.Today)
            {
                throw new ArgumentException("Start date must be greater than today.");
            }
        }
    }
}