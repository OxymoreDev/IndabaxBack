using System.Globalization;
using System.Text;
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

        public List<Candidate> GetCandidates()
        {
            using var reader = new StreamReader("Data/candidats.csv", Encoding.UTF8);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";" });
            return csv.GetRecords<Candidate>().ToList();
        }
    }
}