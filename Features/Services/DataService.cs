using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using WebApplication1.Models;

namespace WebApplication1.Features.Services
{
    // Service pour charger les données
    public class DataService
    {
        public List<JobOffer> GetOffers()
        {
            using var reader = new StreamReader("Data/offres.csv", Encoding.UTF8);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";" });
            return csv.GetRecords<JobOffer>().ToList();
        }

        //public List<Candidate> GetCandidates()
        //{
        //    using var reader = new StreamReader("Data/candidats.csv", Encoding.UTF8);
        //    using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";" });
        //    return csv.GetRecords<Candidate>().ToList();
        //}
        public List<Candidate> GetCandidates()
        {
            var stream = new FileStream("Data/candidats.csv", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            // On utilise Encoding.GetEncoding(1252) ou UTF8 avec détection automatique
            using var reader = new StreamReader(stream, Encoding.UTF8, true);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HeaderValidated = null, // Empêche le crash si un titre manque
                MissingFieldFound = null,
                // C'EST CETTE LIGNE QUI SAUVE TOUT :
                // Elle transforme "Secteur d'activité" en "secteurdactivite" pour le mapping
                PrepareHeaderForMatch = args =>
                    Regex.Replace(args.Header.ToLower(), @"[^a-z0-9]", "")
            };

            using var csv = new CsvReader(reader, config);
            //return csv.GetRecords<Candidate>().ToList();
            return csv.GetRecords<Candidate>().Take(1000).ToList();

        }
    }
}