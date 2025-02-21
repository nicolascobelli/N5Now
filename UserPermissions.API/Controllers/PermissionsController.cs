using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Serilog;
using UserPermissions.Application.Commands.ModifyPermission;
using UserPermissions.Application.Commands.RequestPermission;
using UserPermissions.Application.Queries;
using UserPermissions.Application.DTOs;
using UserPermissions.Application.Queries.GetPermissions;

namespace UserPermissions.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PermissionsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly Serilog.ILogger _logger;

        public PermissionsController(IMediator mediator, Serilog.ILogger logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetPermissions()
        {
            _logger.Information($"Executing {nameof(GetPermissionsQuery)} operation");
            var query = new GetPermissionsQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPut("{permissionId}")]
        public async Task<IActionResult> ModifyPermissions(int permissionId, [FromBody] ModifyPermissionCommand command)
        {
            _logger.Information($"Executing {nameof(ModifyPermissionCommand)} operation");
            command.PermissionId = permissionId;
            var result = await _mediator.Send(command);
            if (!result)
            {
                return NotFound(); // Employee or Permission not found
            }
            return NoContent(); // Placeholder response
        }

        [HttpPost]
        public async Task<IActionResult> RequestPermissions([FromBody] RequestPermissionCommand command)
        {
             _logger.Information($"Executing {nameof(RequestPermissionCommand)} operation");
            var result = await _mediator.Send(command);
            if (result == null)
            {
                return NotFound(); // Employee or Permission type not found
            }
            return Created("", result); // Return the ID of the created permission
        }
    }
}