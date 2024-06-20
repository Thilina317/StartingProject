namespace StartingProject.Data.Models
{
    public class Question
    {
        public string Id { get; set; }
        public string ProgramId { get; set; }
        public string Type { get; set; }
        public string Text { get; set; }
        public List<string> Choices { get; set; }
        public bool IsRequired { get; set; }
    }
}
