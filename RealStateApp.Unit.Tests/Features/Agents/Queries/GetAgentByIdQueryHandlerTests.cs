
namespace RealStateApp.Unit.Tests.Features.Agents.Queries
{
    using AutoMapper;
    using Moq;
    using RealStateApp.Core.Application.Dtos.User;
    using RealStateApp.Core.Application.Exceptions;
    using RealStateApp.Core.Application.Features.Agents.Queries.GetById;
    using RealStateApp.Core.Application.Interfaces;
    using RealStateApp.Core.Domain.Interfaces;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class GetAgentByIdQueryHandlerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IPropertyRepository> _propertyRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetAgentByIdQueryHandler _handler;

        public GetAgentByIdQueryHandlerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _propertyRepositoryMock = new Mock<IPropertyRepository>();
            _mapperMock = new Mock<IMapper>();

            _handler = new GetAgentByIdQueryHandler(
                _userServiceMock.Object,
                _propertyRepositoryMock.Object,
                _mapperMock.Object
            );
        }

        [Fact]
        public async Task Handle_ShouldReturnAgentDto_WhenAgentExists()
        {
            // Arrange
            var agentId = Guid.NewGuid().ToString();

            var userDto = new UserDto
            {
                Id = agentId,
                UserName = "johndoe",
                Dni = "12345678",
                FirstName = "John",
                LastName = "Doe",
                Email = "john@domain.com",
                Role = "AGENT"
            };

            var mappedDto = new AgentDto
            {
                Id = agentId,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@domain.com",
            };

            _userServiceMock
                .Setup(s => s.GetById(agentId))
                .ReturnsAsync(userDto);

            _propertyRepositoryMock
                .Setup(r => r.GetAgentPropertiesCount(agentId))
                .ReturnsAsync(5);

            _mapperMock
                .Setup(m => m.Map<AgentDto>(userDto))
                .Returns(mappedDto);

            var query = new GetAgentByIdQuery { Id = agentId };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(agentId, result.Id);
            Assert.Equal(5, result.PropertiesCount);

            _userServiceMock.Verify(s => s.GetById(agentId), Times.Once);
            _propertyRepositoryMock.Verify(r => r.GetAgentPropertiesCount(agentId), Times.Once);
            _mapperMock.Verify(m => m.Map<AgentDto>(userDto), Times.Once);
        }


        [Fact]
        public async Task Handle_ShouldThrowApiException_WhenAgentDoesNotExist()
        {
            // Arrange
            var agentId = Guid.NewGuid().ToString();

            _userServiceMock
                .Setup(s => s.GetById(agentId))
                .ReturnsAsync((UserDto)null); // Retorna null para simular agente no encontrado

            _propertyRepositoryMock
                .Setup(r => r.GetAgentPropertiesCount(agentId))
                .ReturnsAsync(0);

            var query = new GetAgentByIdQuery { Id = agentId };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ApiException>(() =>
                _handler.Handle(query, CancellationToken.None)
            );

            Assert.Equal("Agent not found with Id", ex.Message);
            _userServiceMock.Verify(s => s.GetById(agentId), Times.Once);
        }
    }

}
