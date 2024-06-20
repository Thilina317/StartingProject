namespace StartingProject.Data.DTOS
{
    public class CreateProgramDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public List<CreateQuestionDto> Questions { get; set; }
    }
}
