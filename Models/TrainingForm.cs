namespace DotNetAssignment2.Models
{
    public class TrainingForm
    {
        public int Id { get; set; }
        public string? FilePath { get; set; }
        public string? Label { get; set; }
        public string? FeatureSelector { get; set; }
        public List<string>? Layers { get; set; } = new List<string>();

        public int Epoch { get; set; }
        public int BatchSize { get; set; }
        public string? Optimizer { get; set; }
        public string? TypeOfTraining { get; set; }

    }
}
