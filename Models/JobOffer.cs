using CsvHelper.Configuration.Attributes;

namespace WebApplication1.Models
{
    public class JobOffer
    {
        [Name("Référence offre")]
        public string Id { get; set; }

        [Name("Intitule")]
        public string Intitule { get; set; }

        [Name("Secteur activité")]
        public string Secteur { get; set; }

        [Name("Entreprise")]
        public string Entreprise { get; set; }

        [Name("Lieu")]
        public string Location { get; set; }

        [Name("Type contrat")]
        public string TypeContrat { get; set; }

        // Note du prof : Comme il n'y a pas de colonne "Compétences" dans ce fichier, 
        // on va matcher sur l'Intitulé et le Secteur d'activité.
    }
}