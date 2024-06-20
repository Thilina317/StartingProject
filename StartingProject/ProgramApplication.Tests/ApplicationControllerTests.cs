using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Moq;
using StartingProject.Controllers;
using StartingProject.Data.DTOS;
using StartingProject.Data.Models;
using Xunit;

namespace StartingProject.ProgramApplication.Tests
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationControllerTests
    {
        private readonly Mock<Container> _containerMock;
        private readonly ApplicationController _controller;

        public ApplicationControllerTests()
        {
            _containerMock = new Mock<Container>();
            _controller = new ApplicationController(_containerMock.Object);
        }

        [Fact]
        public async Task ApplyProgram_ShouldReturnOkResult()
        {
            // Arrange
            var applyProgramDto = new ApplyProgramDto
            {
                ProgramId = "test-program-id",
                Answers = new Dictionary<string, string>
            {
                { "Question 1", "Answer 1" }
            }
            };

            var program = new Programs
            {
                Id = applyProgramDto.ProgramId,
                Title = "Test Program",
                Description = "Test Description",
                Questions = new List<Question>()
            };

            var sqlQueryText = $"SELECT * FROM c WHERE c.id = '{applyProgramDto.ProgramId}'";
            var queryDefinition = new QueryDefinition(sqlQueryText);

            var feedIteratorMock = new Mock<FeedIterator<Programs>>();
            feedIteratorMock.SetupSequence(fi => fi.HasMoreResults)
                .Returns(true)
                .Returns(false);
            feedIteratorMock.Setup(fi => fi.ReadNextAsync(default))
                .ReturnsAsync(new FeedResponse<Programs>(new List<Programs> { program }));

            _containerMock.Setup(c => c.GetItemQueryIterator<Programs>(It.IsAny<QueryDefinition>(), null, null))
                .Returns(feedIteratorMock.Object);

            // Act
            var result = await _controller.ApplyProgram(applyProgramDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var application = Assert.IsType<object>(okResult.Value);
        }

        [Fact]
        public async Task GetApplicationQuestions_ShouldReturnOkResult_WhenProgramExists()
        {
            // Arrange
            var programId = "test-program-id";
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

            var sqlQueryText = $"SELECT * FROM c WHERE c.id = '{programId}'";
            var queryDefinition = new QueryDefinition(sqlQueryText);

            var feedIteratorMock = new Mock<FeedIterator<Programs>>();
            feedIteratorMock.SetupSequence(fi => fi.HasMoreResults)
                .Returns(true)
                .Returns(false);
            feedIteratorMock.Setup(fi => fi.ReadNextAsync(default))
                .ReturnsAsync(new FeedResponse<Programs>(new List<Programs> { program }));

            _containerMock.Setup(c => c.GetItemQueryIterator<Programs>(It.IsAny<QueryDefinition>(), null, null))
                .Returns(feedIteratorMock.Object);

            // Act
            var result = await _controller.GetApplicationQuestions(programId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var questions = Assert.IsType<List<Question>>(okResult.Value);
            Assert.Single(questions);
            Assert.Equal("Test Question", questions[0].Text);
        }

        [Fact]
        public async Task GetApplicationQuestions_ShouldReturnNotFound_WhenProgramDoesNotExist()
        {
            // Arrange
            var programId = "non-existing-id";

            var sqlQueryText = $"SELECT * FROM c WHERE c.id = '{programId}'";
            var queryDefinition = new QueryDefinition(sqlQueryText);

            var feedIteratorMock = new Mock<FeedIterator<Programs>>();
            feedIteratorMock.SetupSequence(fi => fi.HasMoreResults)
                .Returns(false);

            _containerMock.Setup(c => c.GetItemQueryIterator<Programs>(It.IsAny<QueryDefinition>(), null, null))
                .Returns(feedIteratorMock.Object);

            // Act
            var result = await _controller.GetApplicationQuestions(programId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
