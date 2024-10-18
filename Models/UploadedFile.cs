namespace DotNetAssignment2.Models
{
    public class UploadedFile
    {
        public int Id { get; set; }
        public required string FileName { get; set; }
        public required string FilePath { get; set; }
        public DateTime UploadTime { get; set; } = DateTime.Now;
    }
}
