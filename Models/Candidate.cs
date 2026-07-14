using CsvHelper.Configuration.Attributes;

namespace WebApplication1.Models
{
    public class Candidate
    {
        public string Id { get; set; }
        public string Nom { get; set; } // ou ID anonyme
        public string Competences { get; set; }

        [Name("NiveauEtude")]
        public string Education { get; set; }
        public string MetierVise { get; set; }
        public string Secteur { get; set; }
         
        [Name("Localisation")] //[Name("Localisation")] Mobilité
        public string Localisation { get; set; }    

        [Ignore] // <--- On dit à CsvHelper d'ignorer ce champ lors de la lecture du CSV
        public float[]? Embedding { get; set; }
    }
}