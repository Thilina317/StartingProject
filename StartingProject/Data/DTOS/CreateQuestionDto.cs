namespace StartingProject.Data.DTOS
{
    public class CreateQuestionDto
    {
        public string Type { get; set; }
        public string Text { get; set; }
        public List<string> Choices { get; set; }
        public bool IsRequired { get; set; }
    }
}
