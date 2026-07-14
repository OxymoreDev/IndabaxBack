namespace WebApplication1.Models
{
    public class MatchResult
    {
        // On utilise les noms exacts demandés par le jury (Point G)
        public string candidate_id { get; set; }
        public int rank { get; set; }
        public string job_id { get; set; }
        public double score { get; set; }

        // Propriétés bonus pour ton interface Next.js
        public string candidate_name { get; set; }
        public string job_title { get; set; }
        public string company_name { get; set; } // <--- AJOUTE CECI (Entreprise)
        public List<string> common_skills { get; set; }
        public List<string> missing_skills { get; set; }
    }
}