using Application.Interfaces;
using Application.Services;
using Moq;

namespace Application.Tests.Services;

public class DashboardServiceTests
{
    private readonly Mock<IDashboardRepository> _repositoryMock = new(MockBehavior.Strict);
    private readonly DashboardService _service;

    public DashboardServiceTests()
    {
        _service = new DashboardService(_repositoryMock.Object);
    }

    private void SetupRepository(int total, int inAnalysis, int approved, int rejected)
    {
        _repositoryMock.Setup(r => r.GetTotalRequestsAsync()).ReturnsAsync(total);
        _repositoryMock.Setup(r => r.GetRequestsInAnalysisAsync()).ReturnsAsync(inAnalysis);
        _repositoryMock.Setup(r => r.GetApprovedRequestsAsync()).ReturnsAsync(approved);
        _repositoryMock.Setup(r => r.GetRejectedRequestsAsync()).ReturnsAsync(rejected);
    }

    [Fact]
    public async Task GetDashboard_ShouldMapRepositoryValuesToDto()
    {
        SetupRepository(total: 100, inAnalysis: 30, approved: 50, rejected: 20);

        var result = await _service.GetDashboard();

        Assert.NotNull(result);
        Assert.Equal(100, result.TotalRequests);
        Assert.Equal(30, result.RequestsInAnalysis);
        Assert.Equal(50, result.ApprovedRequests);
        Assert.Equal(20, result.RejectedRequests);
    }

    [Fact]
    public async Task GetDashboard_ShouldCallEachRepositoryMethodExactlyOnce()
    {
        SetupRepository(total: 10, inAnalysis: 2, approved: 5, rejected: 3);

        await _service.GetDashboard();

        _repositoryMock.Verify(r => r.GetTotalRequestsAsync(), Times.Once);
        _repositoryMock.Verify(r => r.GetRequestsInAnalysisAsync(), Times.Once);
        _repositoryMock.Verify(r => r.GetApprovedRequestsAsync(), Times.Once);
        _repositoryMock.Verify(r => r.GetRejectedRequestsAsync(), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetDashboard_WhenThereAreNoRequests_ShouldReturnZeros()
    {
        SetupRepository(total: 0, inAnalysis: 0, approved: 0, rejected: 0);

        var result = await _service.GetDashboard();

        Assert.Equal(0, result.TotalRequests);
        Assert.Equal(0, result.RequestsInAnalysis);
        Assert.Equal(0, result.ApprovedRequests);
        Assert.Equal(0, result.RejectedRequests);
    }

    [Theory]
    [InlineData(1, 1, 0, 0)]
    [InlineData(5, 0, 5, 0)]
    [InlineData(7, 0, 0, 7)]
    [InlineData(1000, 250, 600, 150)]
    public async Task GetDashboard_ShouldNotMixUpFields(int total, int inAnalysis, int approved, int rejected)
    {
        SetupRepository(total, inAnalysis, approved, rejected);

        var result = await _service.GetDashboard();

        Assert.Equal(total, result.TotalRequests);
        Assert.Equal(inAnalysis, result.RequestsInAnalysis);
        Assert.Equal(approved, result.ApprovedRequests);
        Assert.Equal(rejected, result.RejectedRequests);
    }

    [Fact]
    public async Task GetDashboard_ShouldQueryRepositoryInExpectedOrder()
    {
        var sequence = new MockSequence();
        _repositoryMock.InSequence(sequence).Setup(r => r.GetTotalRequestsAsync()).ReturnsAsync(4);
        _repositoryMock.InSequence(sequence).Setup(r => r.GetRequestsInAnalysisAsync()).ReturnsAsync(1);
        _repositoryMock.InSequence(sequence).Setup(r => r.GetApprovedRequestsAsync()).ReturnsAsync(2);
        _repositoryMock.InSequence(sequence).Setup(r => r.GetRejectedRequestsAsync()).ReturnsAsync(1);

        var result = await _service.GetDashboard();

        Assert.Equal(4, result.TotalRequests);
    }

    [Fact]
    public async Task GetDashboard_WhenRepositoryThrows_ShouldPropagateExceptionAndStop()
    {
        _repositoryMock.Setup(r => r.GetTotalRequestsAsync()).ReturnsAsync(10);
        _repositoryMock.Setup(r => r.GetRequestsInAnalysisAsync())
            .ThrowsAsync(new InvalidOperationException("db error"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetDashboard());

        Assert.Equal("db error", ex.Message);
        _repositoryMock.Verify(r => r.GetApprovedRequestsAsync(), Times.Never);
        _repositoryMock.Verify(r => r.GetRejectedRequestsAsync(), Times.Never);
    }
}
