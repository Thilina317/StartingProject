using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Moq;
using StartingProject.Controllers;
using StartingProject.Data.DTOS;
using StartingProject.Data.Models;
using Xunit;

namespace ProgramApplication.Tests;

public class ProgramsControllerTests
{
    private readonly Mock<Container> _containerMock;
    private readonly ProgramsController _controller;

    public ProgramsControllerTests()
    {
        _containerMock = new Mock<Container>();
        _controller = new ProgramsController(_containerMock.Object);
    }

    [Fact]
    public async Task CreateProgram_ShouldReturnOkResult()
    {
        // Arrange
        var createProgramDto = new CreateProgramDto
        {
            Title = "Test Program",
            Description = "Test Description",
            Questions = new List<CreateQuestionDto>
            {
                new CreateQuestionDto
                {
                    Type = "Paragraph",
                    Text = "Test Question",
                    Choices = new List<string>(),
                    IsRequired = true
                }
            }
        };

        // Act
        var result = await _controller.CreateProgram(createProgramDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var program = Assert.IsType<Programs>(okResult.Value);
        Assert.Equal("Test Program", program.Title);
        Assert.Equal("Test Description", program.Description);
        Assert.Single(program.Questions);
        Assert.Equal("Test Question", program.Questions[0].Text);
    }

    [Fact]
    public async Task GetProgram_ShouldReturnOkResult_WhenProgramExists()
    {
        // Arrange
        var programId = "test-id";
        var program = new Programs
        {
            Id = programId,
            Title = "Test Program",
            Description = "Test Description",
            Questions = new List<Question>
            {
                new Question
                {
                    Id = "question-id",
                    Type = "Paragraph",
                    Text = "Test Question",
                    Choices = new List<string>(),
                    IsRequired = true
                }
            }
        };

        _containerMock.Setup(c => c.ReadItemAsync<Programs>(It.IsAny<string>(), It.IsAny<PartitionKey>(), null, default))
            .ReturnsAsync(new ItemResponse<Programs>(System.Net.HttpStatusCode.OK)
            {
                Resource = program
            });

        // Act
        var result = await _controller.GetProgram(programId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedProgram = Assert.IsType<Programs>(okResult.Value);
        Assert.Equal(programId, returnedProgram.Id);
        Assert.Equal("Test Program", returnedProgram.Title);
    }

    [Fact]
    public async Task GetProgram_ShouldReturnNotFound_WhenProgramDoesNotExist()
    {
        // Arrange
        var programId = "non-existing-id";

        _containerMock.Setup(c => c.ReadItemAsync<Program>(It.IsAny<string>(), It.IsAny<PartitionKey>(), null, default))
            .ThrowsAsync(new CosmosException("Not Found", System.Net.HttpStatusCode.NotFound, 0, string.Empty, 0));

        // Act
        var result = await _controller.GetProgram(programId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}