namespace MovementMonitoring.Models
{
    public class Violation
    {
        public int Id { get; set; }
        public virtual Movement? Movement { get; set; }
        public DateTime DateTime { get; set; }
        public string? Type { get; set; }
    }
}
