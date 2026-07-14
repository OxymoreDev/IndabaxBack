using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using WebApplication1.Models;

namespace WebApplication1.Features.Services
{
    public class EvaluationMetrics
    {
        public double PrecisionAt5 { get; set; }
        public double RecallAt5 { get; set; }
        public double NdcgAt5 { get; set; } // <--- AJOUTÉ
        public double NdcgAt10 { get; set; } // <--- AJOUTÉ
        public int TotalCandidatesEvaluated { get; set; }
        public string Status { get; set; }
    }

    public class EvaluationService
    {
        public EvaluationMetrics Evaluate(List<MatchResult> predictions, List<GroundTruth> reality)
        {
            if (reality == null || !reality.Any())
                return new EvaluationMetrics { Status = "Fichier de vérité vide." };

            double sumPrecision = 0;
            double sumRecall = 0;
            int k = 5;
            int evaluatedCount = 0;
            double sumNdcg5 = 0, sumNdcg10 = 0;

            // 1. Nettoyage de la réalité
            var realityMap = reality
                .GroupBy(r => r.CandidateId.Trim().ToUpper())
                .ToDictionary(g => g.Key, g => g.Select(x => x.JobId.Trim().ToUpper()).ToList());

            // 2. Groupement de tes prédictions
            var predictionsByCandidate = predictions
                .GroupBy(p => p.candidate_id.Trim().ToUpper());

            foreach (var group in predictionsByCandidate)
            {
                var candId = group.Key;
                if (!realityMap.ContainsKey(candId)) continue;

                evaluatedCount++;
                var trueJobIds = realityMap[candId];

                var predictedIds = group
                    .OrderByDescending(p => p.score)
                    .Take(k)
                    .Select(p => p.job_id.Trim().ToUpper())
                    .ToList();

                var hits = predictedIds.Intersect(trueJobIds).Count();

                // CALCULS (Une seule fois chaque !)
                sumPrecision += (double)hits / k;

                if (trueJobIds.Count > 0)
                {
                    sumRecall += (double)hits / trueJobIds.Count;
                }
                
                // CALCUL NDCG (Le coeur de l'évaluation du classement)
                sumNdcg5 += CalculateNDCG(predictedIds.Take(5).ToList(), trueJobIds, 5);
                sumNdcg10 += CalculateNDCG(predictedIds.Take(10).ToList(), trueJobIds, 10);

                // Debug console pour les 3 premiers
                if (evaluatedCount <= 3)
                {
                    Console.WriteLine($"Cand {candId} | Hits: {hits} | TrueCount: {trueJobIds.Count}");
                }
            }

            return new EvaluationMetrics
            {
                TotalCandidatesEvaluated = evaluatedCount,
                PrecisionAt5 = evaluatedCount > 0 ? Math.Round(sumPrecision / evaluatedCount, 4) : 0,
                RecallAt5 = evaluatedCount > 0 ? Math.Round(sumRecall / evaluatedCount, 4) : 0, // <--- AJOUTÉ
                NdcgAt5 = Math.Round(sumNdcg5 / evaluatedCount, 4),   // <--- AJOUTÉ
                NdcgAt10 = Math.Round(sumNdcg10 / evaluatedCount, 4), // <--- AJOUTÉ
                Status = "Success"
            };
        }



        public List<GroundTruth> LoadGroundTruth(string filePath)
        {
            if (!File.Exists(filePath)) return new List<GroundTruth>();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";", // <--- CHANGE ICI SI C'EST UN POINT-VIRGULE DANS NOTEPAD
                HeaderValidated = null,
                MissingFieldFound = null,
                PrepareHeaderForMatch = args => args.Header.ToLower().Replace("_", "").Trim()
            };

            using var reader = new StreamReader(filePath, Encoding.UTF8);
            using var csv = new CsvReader(reader, config);
            var list = csv.GetRecords<GroundTruth>().ToList();

            // DEBUG : Affiche dans la console pour vérifier
            Console.WriteLine($"VÉRIF : Chargement de {list.Count} lignes de vérité. Première ligne : {list.FirstOrDefault()?.CandidateId} -> {list.FirstOrDefault()?.JobId}");

            foreach (var row in list.Take(10))
            {
                Console.WriteLine(
                    $"Candidate={row.CandidateId}  Job={row.JobId}");
            }

            return list;
        }
        private double CalculateNDCG(List<string> predicted, List<string> actual, int k)
        {
            double dcg = 0;
            for (int i = 0; i < predicted.Count; i++)
            {
                if (actual.Contains(predicted[i]))
                {
                    // Formule : 1 / log2(rang + 1)
                    dcg += 1.0 / Math.Log2(i + 2);
                }
            }

            double idcg = 0;
            int count = Math.Min(actual.Count, k);
            for (int i = 0; i < count; i++)
            {
                idcg += 1.0 / Math.Log2(i + 2);
            }

            return idcg > 0 ? dcg / idcg : 0;
        }
    }

    public class GroundTruth
    {
        [Name("candidate_id")] // À ajuster selon les en-têtes du fichier jury
        public string CandidateId { get; set; }

        [Name("job_id")]
        public string JobId { get; set; }
    }
}