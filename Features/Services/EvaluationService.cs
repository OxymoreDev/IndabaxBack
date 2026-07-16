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
        public EvaluationMetrics Evaluate(List<MatchResult> predictions, List<GroundTruth> realityRows)
        {
            if (realityRows == null || !realityRows.Any())
                return new EvaluationMetrics { Status = "Fichier de vérité vide." };

            double sumPrecision = 0;
            double sumRecall = 0;
            double sumNdcg5 = 0;
            double sumNdcg10 = 0;
            int k = 5;
            int evaluatedCount = 0;

            // 1. On utilise GroupBy pour fusionner les lignes si un candidat apparaît plusieurs fois
            var realityMap = realityRows
                .Where(r => !string.IsNullOrEmpty(r.CandidateId))
                .GroupBy(r => r.CandidateId.Trim().ToUpper())
                .ToDictionary(
                    g => g.Key,
                    g => g.SelectMany(r => new[] { r.JobId1, r.JobId2, r.JobId3 }) // On aplatit toutes les colonnes d'offres
                          .Where(id => !string.IsNullOrWhiteSpace(id))
                          .Select(id => id.Trim().ToUpper())
                          .Distinct() // On enlève les doublons d'offres pour un même candidat
                          .ToList()
                );

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
                    .Select(p => p.job_id.Trim().ToUpper())
                    .ToList();

                // Calcul des hits sur le Top 5
                var top5 = predictedIds.Take(5).ToList();
                var hits = top5.Intersect(trueJobIds).Count();

                sumPrecision += (double)hits / k;
                sumRecall += (double)hits / trueJobIds.Count;
                sumNdcg5 += CalculateNDCG(top5, trueJobIds, 5);
                sumNdcg10 += CalculateNDCG(predictedIds.Take(10).ToList(), trueJobIds, 10);
            }

            return new EvaluationMetrics
            {
                TotalCandidatesEvaluated = evaluatedCount,
                PrecisionAt5 = Math.Round(sumPrecision / evaluatedCount, 4),
                RecallAt5 = Math.Round(sumRecall / evaluatedCount, 4),
                NdcgAt5 = Math.Round(sumNdcg5 / evaluatedCount, 4),
                NdcgAt10 = Math.Round(sumNdcg10 / evaluatedCount, 4),
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
            Console.WriteLine($"VÉRIF : Chargement de {list.Count} lignes de vérité. Première ligne : {list.FirstOrDefault()?.CandidateId} -> {list.FirstOrDefault()?.JobId1}");

            foreach (var row in list.Take(10))
            {
                Console.WriteLine(
                    $"Candidate={row.CandidateId}  Job={row.JobId1}");
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
        [Name("id_demandeur")]
        public string CandidateId { get; set; }

        [Name("id_offre1")]
        public string JobId1 { get; set; }

        [Name("id_offre2")]
        public string JobId2 { get; set; }

        [Name("id_offre3")]
        public string JobId3 { get; set; }
    }
}