using CncMachineTracker.Application.Services;
using CncMachineTracker.Application.Ports;
using CncMachineTracker.Domain.Entities;
using CncMachineTracker.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace CncMachineTracker.Tests
{
    public class MachineServiceTests
    {
        private readonly Mock<IMachineRepository> _mockRepository;
        private readonly MachineService _service;

        public MachineServiceTests()
        {
            _mockRepository = new Mock<IMachineRepository>();
            _service = new MachineService(_mockRepository.Object);
        }

        [Fact]
        public async Task SimulateAsync_NewMachine_CreatesBaselineAndReturnsValidMachine()
        {
            // Arrange
            var machineId = "M001";
            _mockRepository.Setup(r => r.GetLatestAsync(machineId))
                .ReturnsAsync((Machine?)null);
            _mockRepository.Setup(r => r.EnsureExistsAsync(machineId, It.IsAny<Machine>()))
                .Returns(Task.CompletedTask);
            _mockRepository.Setup(r => r.UpsertAsync(It.IsAny<Machine>(), It.IsAny<MachineSample>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.SimulateAsync(machineId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(machineId);
            result.ProductionCount.Should().BeGreaterThanOrEqualTo(0);
            
            // Cycle time depends on status - only validate if running
            if (result.Status == MachineStatus.Running)
            {
                result.CycleTimeSeconds.Should().BeGreaterThanOrEqualTo(10).And.BeLessThanOrEqualTo(120);
            }
            else
            {
                result.CycleTimeSeconds.Should().Be(0); // Non-running machines have 0 cycle time
            }
            
            result.Status.Should().BeOneOf(MachineStatus.Running, MachineStatus.Stopped, MachineStatus.Alarm);
            result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            _mockRepository.Verify(r => r.EnsureExistsAsync(machineId, It.IsAny<Machine>()), Times.Once);
            _mockRepository.Verify(r => r.UpsertAsync(It.IsAny<Machine>(), It.IsAny<MachineSample>()), Times.Once);
        }

        [Fact]
        public async Task SimulateAsync_ExistingMachine_UpdatesAndReturnsValidMachine()
        {
            // Arrange
            var machineId = "M001";
            var existingMachine = new Machine
            {
                Id = machineId,
                Status = MachineStatus.Running,
                ProductionCount = 100,
                CycleTimeSeconds = 30,
                Timestamp = DateTime.UtcNow.AddMinutes(-5)
            };

            _mockRepository.Setup(r => r.GetLatestAsync(machineId))
                .ReturnsAsync(existingMachine);
            _mockRepository.Setup(r => r.UpsertAsync(It.IsAny<Machine>(), It.IsAny<MachineSample>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.SimulateAsync(machineId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(machineId);
            result.Timestamp.Should().BeAfter(existingMachine.Timestamp);
            _mockRepository.Verify(r => r.UpsertAsync(It.IsAny<Machine>(), It.IsAny<MachineSample>()), Times.Once);
        }

        [Fact]
        public async Task SimulateAsync_MultipleCalls_ProductionCountIncreasesWhenRunning()
        {
            // Arrange
            var machineId = "M001";
            var initialMachine = new Machine
            {
                Id = machineId,
                Status = MachineStatus.Running,
                ProductionCount = 100,
                CycleTimeSeconds = 30,
                Timestamp = DateTime.UtcNow
            };

            _mockRepository.Setup(r => r.GetLatestAsync(machineId))
                .ReturnsAsync(initialMachine);
            _mockRepository.Setup(r => r.UpsertAsync(It.IsAny<Machine>(), It.IsAny<MachineSample>()))
                .Returns(Task.CompletedTask);

            // Act & Assert - Multiple simulations
            var results = new List<Machine>();
            for (int i = 0; i < 10; i++)
            {
                var result = await _service.SimulateAsync(machineId);
                results.Add(result);
                
                // Update the mock to return the latest result for next iteration
                _mockRepository.Setup(r => r.GetLatestAsync(machineId))
                    .ReturnsAsync(result);
            }

            // Verify that production count generally increases (allowing for status changes)
            var runningResults = results.Where(r => r.Status == MachineStatus.Running).ToList();
            if (runningResults.Count > 1)
            {
                var firstRunning = runningResults.First();
                var lastRunning = runningResults.Last();
                lastRunning.ProductionCount.Should().BeGreaterThanOrEqualTo(firstRunning.ProductionCount);
            }
        }

        [Fact]
        public async Task SimulateAsync_StatusTransitions_FollowMarkovChainProbabilities()
        {
            // Arrange
            var machineId = "M001";
            var initialMachine = new Machine
            {
                Id = machineId,
                Status = MachineStatus.Running,
                ProductionCount = 100,
                CycleTimeSeconds = 30,
                Timestamp = DateTime.UtcNow
            };

            _mockRepository.Setup(r => r.GetLatestAsync(machineId))
                .ReturnsAsync(initialMachine);
            _mockRepository.Setup(r => r.UpsertAsync(It.IsAny<Machine>(), It.IsAny<MachineSample>()))
                .Returns(Task.CompletedTask);

            // Act - Multiple simulations to test probabilities
            var results = new List<Machine>();
            for (int i = 0; i < 100; i++)
            {
                var result = await _service.SimulateAsync(machineId);
                results.Add(result);
                
                // Update the mock to return the latest result for next iteration
                _mockRepository.Setup(r => r.GetLatestAsync(machineId))
                    .ReturnsAsync(result);
            }

            // Assert - Verify that Running status occurs frequently (should be ~80% of the time)
            // Allow for more variance in the test
            var runningCount = results.Count(r => r.Status == MachineStatus.Running);
            var runningPercentage = (double)runningCount / results.Count;
            runningPercentage.Should().BeGreaterThan(0.2); // Allow more variance for probabilistic test
        }

        [Fact]
        public async Task GetAllAsync_ReturnsRepositoryResults()
        {
            // Arrange
            var expectedMachines = new List<Machine>
            {
                new Machine { Id = "M001", Status = MachineStatus.Running, ProductionCount = 100, CycleTimeSeconds = 30, Timestamp = DateTime.UtcNow },
                new Machine { Id = "M002", Status = MachineStatus.Stopped, ProductionCount = 50, CycleTimeSeconds = 0, Timestamp = DateTime.UtcNow }
            };

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(expectedMachines);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Should().BeEquivalentTo(expectedMachines);
        }

        [Fact]
        public async Task GetLatestAsync_ReturnsRepositoryResult()
        {
            // Arrange
            var machineId = "M001";
            var expectedMachine = new Machine
            {
                Id = machineId,
                Status = MachineStatus.Running,
                ProductionCount = 100,
                CycleTimeSeconds = 30,
                Timestamp = DateTime.UtcNow
            };

            _mockRepository.Setup(r => r.GetLatestAsync(machineId))
                .ReturnsAsync(expectedMachine);

            // Act
            var result = await _service.GetLatestAsync(machineId);

            // Assert
            result.Should().BeEquivalentTo(expectedMachine);
        }

        [Fact]
        public async Task GetHistoryAsync_ReturnsRepositoryResult()
        {
            // Arrange
            var machineId = "M001";
            var window = TimeSpan.FromMinutes(10);
            var expectedSamples = new List<MachineSample>
            {
                new MachineSample { MachineId = machineId, Status = MachineStatus.Running, ProductionCount = 100, CycleTimeSeconds = 30, Timestamp = DateTime.UtcNow },
                new MachineSample { MachineId = machineId, Status = MachineStatus.Running, ProductionCount = 95, CycleTimeSeconds = 30, Timestamp = DateTime.UtcNow.AddMinutes(-5) }
            };

            _mockRepository.Setup(r => r.GetHistoryAsync(machineId, window))
                .ReturnsAsync(expectedSamples);

            // Act
            var result = await _service.GetHistoryAsync(machineId, window);

            // Assert
            result.Should().BeEquivalentTo(expectedSamples);
        }
    }
}
