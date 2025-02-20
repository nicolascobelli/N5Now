using MediatR;
using System;

namespace UserPermissions.Application.Commands.RequestPermission
{
    public class RequestPermissionCommand : IRequest<int?>
    {
        public int EmployeeId { get; set; }
        public int PermissionTypeId { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
