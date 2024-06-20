using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using StartingProject.Data.DTOS;
using StartingProject.Data.Models;
using System.Text.Json;

namespace StartingProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProgramsController : ControllerBase
    {
        private readonly Container _container;

        public ProgramsController(Container container)
        {
            _container = container;
        }

        [HttpPost]
        public async Task<IActionResult> CreateProgram([FromBody] CreateProgramDto dto)
        {
            var program = new Programs
            {
                Id = Guid.NewGuid().ToString(),
                Title = dto.Title,
                Description = dto.Description,
                Questions = dto.Questions.Select(q => new Question
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = q.Type,
                    Text = q.Text,
                    Choices = q.Choices,
                    IsRequired = q.IsRequired
                }).ToList()
            };

            await _container.CreateItemAsync(program);
            return Ok(program);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProgram(string id, [FromBody] CreateProgramDto dto)
        {
            var sqlQueryText = $"SELECT * FROM c WHERE c.id = '{id}'";
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

                program.Title = dto.Title;
                program.Description = dto.Description;
                program.Questions = dto.Questions.Select(q => new Question
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = q.Type,
                    Text = q.Text,
                    Choices = q.Choices,
                    IsRequired = q.IsRequired
                }).ToList();

                await _container.ReplaceItemAsync(program, program.Id);
                return Ok(program);
            }

            return NotFound();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProgram(string id)
        {
            try
            {
                var response = await _container.ReadItemAsync<Programs>(id, new PartitionKey(id));
                return Ok(response.Resource);
            }
            catch (CosmosException)
            {
                return NotFound();
            }
        }
    }
}
