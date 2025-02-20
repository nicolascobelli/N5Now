using MediatR;
using System;

namespace UserPermissions.Application.Commands.ModifyPermission
{
    public class ModifyPermissionCommand : IRequest<bool>
    {
        public int PermissionId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
