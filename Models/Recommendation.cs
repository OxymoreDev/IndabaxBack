namespace WebApplication1.Models
{
    public class Recommendation
    {
        public string candidate_id { get; set; }
        public int rank { get; set; }
        public string job_id { get; set; }
        public double score { get; set; }
    }
}