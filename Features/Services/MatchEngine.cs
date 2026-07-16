using Microsoft.ML;
using WebApplication1.Models;

namespace WebApplication1.Features.Services
{
    public class MatchEngine
    {
        private readonly MLContext _mlContext;
        private readonly MatchingService _matchingService;

      

        private static readonly Dictionary<string, string> KnowledgeBase = new(StringComparer.OrdinalIgnoreCase)
        {
            // Finance & Gestion
            { "comptable", "comptabilité finance audit excel sage fiscalité gestion" },
            { "caissier", "caisse encaissement accueil vente client facturation" },
            { "controleur de gestion", "budget reporting analyse financière performance excel" },
            { "fiscaliste", "droit fiscal taxes audit déclarations juridique" },

            // Transport & Logistique
            { "chauffeur", "conduite permis transport logistique véhicule livraison sécurité" },
            { "magasinier", "stock inventaire logistique entrepôt manutention réception" },
            { "gestionnaire de stock", "approvisionnement inventaire sap excel logistique" },

            // Services & Restauration
            { "chef de salle", "restauration service client équipe accueil" },
            { "cuisinier", "cuisine hygiène haccp gastronomie préparation" },
            { "manager", "management leadership équipe planning stratégie" },

            // Technique & Informatique
            { "développeur .net", "c# dotnet sql visual-studio web api backend" },
            { "développeur front", "react javascript typescript css html design" },
            { "administrateur réseaux", "sécurité cisco infrastructure maintenance télécom" },
            { "technicien maintenance", "mécanique dépannage réparation industriel maintenance" }
        };

        public MatchEngine()
        {
            _mlContext = new MLContext();
            _matchingService = new MatchingService();
        }



        

        public List<MatchResult> GetTopKMatches(List<Candidate> candidates, List<JobOffer> offers, int k = 5)
        {
            var finalResults = new System.Collections.Concurrent.ConcurrentBag<MatchResult>();

            // 1. Pré-préparation des textes (très rapide)
            var candidateTexts = candidates.Select(c => EnrichText($"{(c.MetierVise ?? "")} {(c.QualificationMetier ?? "")}")).ToList();
            var offerTexts = offers.Select(o => EnrichText($"{(o.Intitule ?? "")} {(o.Secteur ?? "")}")).ToList();

            var candidateWordSets = candidateTexts.Select(t => SimpleTokenize(t)).ToList();
            var offerWordSets = offerTexts.Select(t => SimpleTokenize(t)).ToList();

            // 2. Transformation ML.NET Batch (Une seule fois)
            var allData = candidateTexts.Concat(offerTexts).Select(t => new TextData { Text = t }).ToList();
            var dv = _mlContext.Data.LoadFromEnumerable(allData);
            var pipeline = _mlContext.Transforms.Text.FeaturizeText("Features", "Text");
            var model = pipeline.Fit(dv);
            var transformed = model.Transform(dv);
            var allVectors = _mlContext.Data.CreateEnumerable<TransformedData>(transformed, false).ToList();

            var candidateVectors = allVectors.Take(candidates.Count).Select(v => v.Features).ToList();
            var offerVectors = allVectors.Skip(candidates.Count).Select(v => v.Features).ToList();

            // 3. BOUCLE PARALLÈLE (Utilise 100% de ton CPU pour aller 8x plus vite)
            Parallel.For(0, candidates.Count, i =>
            {
                var matchesForCandidate = new List<MatchResult>();
                var cVector = candidateVectors[i];
                var cWords = candidateWordSets[i];

                for (int j = 0; j < offers.Count; j++)
                {
                    // Calcul mathématique pur (Cosinus)
                    double score = _matchingService.CalculateScore(cVector, offerVectors[j]);

                    if (score > 0.1)
                    {
                        matchesForCandidate.Add(new MatchResult
                        {
                            candidate_id = candidates[i].Id,
                            candidate_name = candidates[i].Id,
                            job_id = offers[j].Id,
                            job_title = offers[j].Intitule,
                            company_name = offers[j].Entreprise,
                            score = Math.Round(score, 4),
                            common_skills = cWords.Intersect(offerWordSets[j]).ToList(),
                            missing_skills = offerWordSets[j].Except(cWords).Take(3).ToList()
                        });
                    }
                }

                // On trie et on prend le Top K
                var topK = matchesForCandidate.OrderByDescending(m => m.score)
                                              .GroupBy(m => m.job_id).Select(g => g.First())
                                              .Select((m, idx) => { m.rank = idx + 1; return m; })
                                              .Take(k);

                foreach (var m in topK) finalResults.Add(m);
            });

            return finalResults.OrderBy(r => r.candidate_id).ThenBy(r => r.rank).ToList();
        }

        private string EnrichText(string input)
        {
            string enriched = input.ToLower();
            foreach (var item in KnowledgeBase)
            {
                if (enriched.Contains(item.Key)) enriched += " " + item.Value;
            }
            return enriched;
        }

        private HashSet<string> SimpleTokenize(string text)
        {
            return text.ToLower().Split(new[] { ' ', ',', ';', '.' }, StringSplitOptions.RemoveEmptyEntries)
                       .Where(w => w.Length > 2).ToHashSet();
        }

        private class TextData { public string Text { get; set; } }
        private class TransformedData { public float[] Features { get; set; } }
    }
}