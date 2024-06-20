using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using StartingProject.Data.DTOS;
using StartingProject.Data.Models;

namespace StartingProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationController : ControllerBase
    {
        private readonly Container _container;

        public ApplicationController(Container container)
        {
            _container = container;
        }

        [HttpPost]
        public async Task<IActionResult> ApplyProgram([FromBody] ApplyProgramDto dto)
        {
            var sqlQueryText = $"SELECT * FROM c WHERE c.id = '{dto.ProgramId}'";
            var queryDefinition = new QueryDefinition(sqlQueryText);
            var queryResultSetIterator = _container.GetItemQueryIterator<Programs>(queryDefinition);

            if (queryResultSetIterator.HasMoreResults)
            {
                var response = await queryResultSetIterator.ReadNextAsync();
                var program = response.FirstOrDefault();

                if (program == null)
                {
                    return NotFound();
                }

                var application = new
                {
                    Id = Guid.NewGuid().ToString(),
                    ProgramId = dto.ProgramId,
                    Answers = dto.Answers
                };

                await _container.CreateItemAsync(application, new PartitionKey(dto.ProgramId));
                return Ok(application);
            }

            return NotFound();
        }

        [HttpGet("{programId}")]
        public async Task<IActionResult> GetApplicationQuestions(string programId)
        {
            var sqlQueryText = $"SELECT * FROM c WHERE c.id = '{programId}'";
            var queryDefinition = new QueryDefinition(sqlQueryText);
            var queryResultSetIterator = _container.GetItemQueryIterator<Programs>(queryDefinition);

            if (queryResultSetIterator.HasMoreResults)
            {
                var response = await queryResultSetIterator.ReadNextAsync();
                var program = response.FirstOrDefault();

                if (program == null)
                {
                    return NotFound();
                }

                return Ok(program.Questions);
            }

            return NotFound();
        }
    }
}
