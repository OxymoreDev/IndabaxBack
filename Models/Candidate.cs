using CsvHelper.Configuration.Attributes;

public class Candidate
{
    [Name("matricule")] // Sera "matricule" après nettoyage
    public string Id { get; set; }

    [Name("secteurdactivite")] // Correspond à "Secteur d'activité" nettoyé
    public string Secteur { get; set; }

    [Name("metiervisequalificationvisee")] // Correspond à "Métier visé / Qualification visée"
    public string MetierVise { get; set; }

    [Name("mobilitegeographique")] // Correspond à "Mobilité géographique"
    public string Localisation { get; set; }

    [Name("qualificationmetier")]
    public string QualificationMetier { get; set; }

    [Name("filierespecialite")]
    public string Specialite { get; set; }

    [Name("diplome")]
    public string Education { get; set; }

    [Ignore]
    public string Nom { get; set; } = "Candidat";

    [Ignore]
    public string Competences => $"{QualificationMetier} {Specialite} {MetierVise}";
}