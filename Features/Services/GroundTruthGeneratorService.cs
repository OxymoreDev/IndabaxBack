using System.Text;
using WebApplication1.Models;

namespace WebApplication1.Features.Services
{
    public class GroundTruthGeneratorService
    {
        public void Generate(List<Candidate> candidates, List<JobOffer> offers)
        {
            var sb = new StringBuilder();
            sb.AppendLine("candidate_id;job_id");

            foreach (var candidate in candidates)
            {
                // Le "Recruteur Humain" choisit les 3 meilleures offres selon des règles simples
                var top3Truth = offers
                    .Select(offer => new {
                        JobId = offer.Id,
                        Score = CalculateHumanExpertScore(candidate, offer)
                    })
                    .Where(x => x.Score > 0)
                    .OrderByDescending(x => x.Score)
                    .Take(3) // On définit que la vérité terrain, c'est le top 3 "humain"
                    .ToList();

                foreach (var truth in top3Truth)
                {
                    sb.AppendLine($"{candidate.Id};{truth.JobId}");
                }
            }

            string path = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Appariement_Demandeurs_Offres.csv");
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            Console.WriteLine($"[VÉRITÉ TERRAIN] Fichier généré : {path}");
        }

        

        private int CalculateHumanExpertScore(Candidate c, JobOffer o)
        {
            int score = 0;

            // On utilise l'opérateur ?? "" pour remplacer les nulls par du texte vide
            string cMetier = c.MetierVise ?? "";
            string oIntitule = o.Intitule ?? "";
            string cSecteur = c.Secteur ?? "";
            string oSecteur = o.Secteur ?? "";
            string cLoc = c.Localisation ?? "";
            string oLoc = o.Location ?? "";

            // 1. Match Métier (seulement si les deux ne sont pas vides)
            if (!string.IsNullOrEmpty(cMetier) && !string.IsNullOrEmpty(oIntitule))
            {
                if (oIntitule.Contains(cMetier, StringComparison.OrdinalIgnoreCase) ||
                    cMetier.Contains(oIntitule, StringComparison.OrdinalIgnoreCase))
                    score += 60;
            }

            // 2. Match Secteur
            if (!string.IsNullOrEmpty(cSecteur) && cSecteur.Equals(oSecteur, StringComparison.OrdinalIgnoreCase))
                score += 30;

            // 3. Match Localisation
            if (!string.IsNullOrEmpty(cLoc) && cLoc.Equals(oLoc, StringComparison.OrdinalIgnoreCase))
                score += 10;

            return score;
        }
    }
}

