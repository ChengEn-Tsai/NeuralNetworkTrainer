namespace DotNetAssignment2.Models
{
    public class UploadedFile
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadTime { get; set; } = DateTime.Now;
    }
}
