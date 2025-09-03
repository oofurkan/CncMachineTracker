using CncMachineTracker.Infrastructure.Persistence;
using CncMachineTracker.Domain.Entities;
using CncMachineTracker.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace CncMachineTracker.Tests
{
    public class RepositoryTests
    {
        private readonly InMemoryMachineRepository _repository;

        public RepositoryTests()
        {
            _repository = new InMemoryMachineRepository();
        }

        [Fact]
        public async Task UpsertAsync_NewMachine_UpdatesLatestAndAddsToHistory()
        {
            // Arrange
            var machine = new Machine
            {
                Id = "M001",
                Status = MachineStatus.Running,
                ProductionCount = 100,
                CycleTimeSeconds = 30,
                Timestamp = DateTime.UtcNow
            };

            var sample = new MachineSample
            {
                MachineId = "M001",
                Status = MachineStatus.Running,
                ProductionCount = 100,
                CycleTimeSeconds = 30,
                Timestamp = DateTime.UtcNow
            };

            // Act
            await _repository.UpsertAsync(machine, sample);

            // Assert
            var latest = await _repository.GetLatestAsync("M001");
            latest.Should().NotBeNull();
            latest!.Id.Should().Be("M001");
            latest.Status.Should().Be(MachineStatus.Running);

            var history = await _repository.GetHistoryAsync("M001", TimeSpan.FromMinutes(10));
            history.Should().HaveCount(1);
            history[0].MachineId.Should().Be("M001");
        }

        [Fact]
        public async Task UpsertAsync_ExistingMachine_UpdatesLatestAndAppendsToHistory()
        {
            // Arrange
            var machine1 = new Machine
            {
                Id = "M001",
                Status = MachineStatus.Running,
                ProductionCount = 100,
                CycleTimeSeconds = 30,
                Timestamp = DateTime.UtcNow.AddMinutes(-30) // 30 minutes ago
            };

            var sample1 = new MachineSample
            {
                MachineId = "M001",
                Status = MachineStatus.Running,
                ProductionCount = 100,
                CycleTimeSeconds = 30,
                Timestamp = DateTime.UtcNow.AddMinutes(-30) // 30 minutes ago
            };

            var machine2 = new Machine
            {
                Id = "M001",
                Status = MachineStatus.Stopped,
                ProductionCount = 105,
                CycleTimeSeconds = 0,
                Timestamp = DateTime.UtcNow
            };

            var sample2 = new MachineSample
            {
                MachineId = "M001",
                Status = MachineStatus.Stopped,
                ProductionCount = 105,
                CycleTimeSeconds = 0,
                Timestamp = DateTime.UtcNow
            };

            // Act
            await _repository.UpsertAsync(machine1, sample1);
            await _repository.UpsertAsync(machine2, sample2);

            // Assert
            var latest = await _repository.GetLatestAsync("M001");
            latest.Should().NotBeNull();
            latest!.Status.Should().Be(MachineStatus.Stopped);
            latest.ProductionCount.Should().Be(105);

            var history = await _repository.GetHistoryAsync("M001", TimeSpan.FromMinutes(10));
            history.Should().HaveCount(1); // Only recent sample within 10 minutes
            history[0].Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            // Check with longer window to see both samples
            var longHistory = await _repository.GetHistoryAsync("M001", TimeSpan.FromMinutes(60));
            longHistory.Should().HaveCount(2);
            longHistory.Should().BeInDescendingOrder(x => x.Timestamp);
        }

        [Fact]
        public async Task GetHistoryAsync_RespectsTimeWindow()
        {
            // Arrange
            var machine = new Machine
            {
                Id = "M001",
                Status = MachineStatus.Running,
                ProductionCount = 100,
                CycleTimeSeconds = 30,
                Timestamp = DateTime.UtcNow
            };

            var oldSample = new MachineSample
            {
                MachineId = "M001",
                Status = MachineStatus.Running,
                ProductionCount = 95,
                CycleTimeSeconds = 30,
                Timestamp = DateTime.UtcNow.AddMinutes(-15) // 15 minutes ago
            };

            var recentSample = new MachineSample
            {
                MachineId = "M001",
                Status = MachineStatus.Running,
                ProductionCount = 100,
                CycleTimeSeconds = 30,
                Timestamp = DateTime.UtcNow
            };

            // Act
            await _repository.UpsertAsync(machine, oldSample);
            await _repository.UpsertAsync(machine, recentSample);

            // Assert - 10 minute window should only include recent sample
            var history = await _repository.GetHistoryAsync("M001", TimeSpan.FromMinutes(10));
            history.Should().HaveCount(1);
            history[0].Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            // Assert - 20 minute window should include both samples
            var longHistory = await _repository.GetHistoryAsync("M001", TimeSpan.FromMinutes(20));
            longHistory.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllMachinesEndpoint_ReturnsMachineList()
        {
            // Arrange
            var machine1 = new Machine
            {
                Id = "M001",
                Status = MachineStatus.Running,
                ProductionCount = 100,
                CycleTimeSeconds = 30,
                Timestamp = DateTime.UtcNow
            };

            var machine2 = new Machine
            {
                Id = "M002",
                Status = MachineStatus.Stopped,
                ProductionCount = 50,
                CycleTimeSeconds = 0,
                Timestamp = DateTime.UtcNow
            };

            var sample1 = new MachineSample
            {
                MachineId = "M001",
                Status = MachineStatus.Running,
                ProductionCount = 100,
                CycleTimeSeconds = 30,
                Timestamp = DateTime.UtcNow
            };

            var sample2 = new MachineSample
            {
                MachineId = "M002",
                Status = MachineStatus.Stopped,
                ProductionCount = 50,
                CycleTimeSeconds = 0,
                Timestamp = DateTime.UtcNow
            };

            // Act
            await _repository.UpsertAsync(machine1, sample1);
            await _repository.UpsertAsync(machine2, sample2);

            // Assert
            var allMachines = await _repository.GetAllAsync();
            allMachines.Should().HaveCount(2);
            allMachines.Should().Contain(m => m.Id == "M001");
            allMachines.Should().Contain(m => m.Id == "M002");
        }

        [Fact]
        public async Task EnsureExistsAsync_CreatesMachineIfNotExists()
        {
            // Arrange
            var machine = new Machine
            {
                Id = "M001",
                Status = MachineStatus.Running,
                ProductionCount = 100,
                CycleTimeSeconds = 30,
                Timestamp = DateTime.UtcNow
            };

            // Act
            await _repository.EnsureExistsAsync("M001", machine);

            // Assert
            var latest = await _repository.GetLatestAsync("M001");
            latest.Should().NotBeNull();
            latest!.Id.Should().Be("M001");
        }

        [Fact]
        public async Task GetLatestAsync_NonExistentMachine_ReturnsNull()
        {
            // Act
            var result = await _repository.GetLatestAsync("NONEXISTENT");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetHistoryAsync_NonExistentMachine_ReturnsEmptyList()
        {
            // Act
            var result = await _repository.GetHistoryAsync("NONEXISTENT", TimeSpan.FromMinutes(10));

            // Assert
            result.Should().BeEmpty();
        }
    }
}
