namespace MovementMonitoring.Models
{
    public class Movement
    {
        public int Id { get; set; }
        public virtual Room? Room { get; set; }
        public virtual Camera? Camera { get; set; }
        public virtual Person? CurrentPerson { get; set; }
        public virtual Person? MostSimilarPerson { get; set; }
        public string? LastPhotoPath { get; set; }
        public string? MostSimilarPhotoPath { get; set; }
        public float FirstDetectionSimilarity { get; set; }
        public float LastDetectionSimilarity { get; set; }
        public DateTime EnteringTime { get; set; }
        public DateTime LastDetectionTime { get; set; }
        public DateTime? LeavingTime { get; set; }
        public bool IsViolation { get; set; }
    }
}
