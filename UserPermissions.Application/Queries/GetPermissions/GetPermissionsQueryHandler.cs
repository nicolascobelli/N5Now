using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UserPermissions.Application.DTOs;
using UserPermissions.Application.Repositories;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

namespace UserPermissions.Application.Queries.GetPermissions
{
    public class GetPermissionsQueryHandler : IRequestHandler<GetPermissionsQuery, List<PermissionDto>>
    {
        private readonly IPermissionsReadRepository _permissionsReadRepository;
        private readonly IProducer<string, string> _producer;
        private readonly string _kafkaTopic;

        public GetPermissionsQueryHandler(IPermissionsReadRepository permissionsReadRepository, IProducer<string, string> producer, IConfiguration configuration)
        {
            _permissionsReadRepository = permissionsReadRepository;
            _producer = producer;
            _kafkaTopic = configuration["Kafka:TopicName"];
        }

        public async Task<List<PermissionDto>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
        {
            var result = await _permissionsReadRepository.GetPermissionsByEmployeeIdAsync(request.EmployeeId, cancellationToken);

            var message = new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = "GetPermissions"
            };
            await _producer.ProduceAsync(_kafkaTopic, message);

            //en este no entiendo que deberia persistir a elastic

            return result;
        }
    }
}
