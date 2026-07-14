namespace WebApplication1.Models
{
    using System.Text;

    public static class DataGenerator
    {
        public static void GenerateFiles()
        {
            var random = new Random();

            var villes = new[]
            {
                "Brazzaville",
                "Pointe-Noire",
                "Dolisie",
                "Nkayi",
                "Ouesso"
            };

            var noms = new[]
            {
                "Nkouka","Mabiala","Tchicaya","Loussakou",
                "Bouanga","Makosso","Zola","Mviri","Kongo","Milandou"
            };

            var prenoms = new[]
            {
                "Arnaud","Grace","Christ","Syntyche",
                "Dieuveil","Marien","Prisca","Gloire",
                "Exauce","Belvina"
            };

            // Secteur -> Métiers -> Compétences

            var jobs = new Dictionary<string, Dictionary<string, string[]>>
            {
                {
                    "Hôtellerie - Restauration",
                    new Dictionary<string,string[]>
                    {
                        { "Chef de salle", new[]{ "Service client","Leadership","Gestion","Communication","Organisation"} },
                        { "Cuisinier", new[]{ "Cuisine","Hygiène","Gestion des stocks","Travail en équipe","Créativité"} },
                        { "Serveur", new[]{ "Service","Accueil","Communication","Encaissement","Relation client"} },
                        { "Manager Hôtel", new[]{ "Management","Planning","Service client","Leadership","Budget"} }
                    }
                },

                {
                    "Transport, Logistique & Supply Chain",
                    new Dictionary<string,string[]>
                    {
                        { "Chauffeur", new[]{ "Conduite","Sécurité","Permis","Ponctualité","Entretien véhicule"} },
                        { "Gestionnaire de stock", new[]{ "SAP","Excel","Inventaire","Organisation","Logistique"} },
                        { "Coordonnateur logistique", new[]{ "Supply Chain","Excel","SAP","Transport","Planification"} }
                    }
                },

        {
            "Finance, Banque, Assurance, Comptabilité & Fiscalité",
            new Dictionary<string,string[]>
            {
                { "Comptable senior", new[]{ "Comptabilité","Excel","Fiscalité","SAGE","Analyse financière"} },
                { "Contrôleur de gestion", new[]{ "Excel","Power BI","Budget","Reporting","Analyse"} },
                { "Fiscaliste", new[]{ "Fiscalité","Droit","Audit","Déclarations","Comptabilité"} }
            }
        },

        {
            "Pétrole, Gaz & Hydrocarbures",
            new Dictionary<string,string[]>
            {
                { "Ingénieur Forage", new[]{ "Forage","HSE","Pétrole","Maintenance","Anglais"} },
                { "Technicien Maintenance", new[]{ "Maintenance","Hydraulique","Mécanique","Sécurité","Diagnostic"} },
                { "Superviseur HSE", new[]{ "HSE","Sécurité","Audit","Normes","Formation"} }
            }
        },

        {
            "Conseil, Stratégie & Gestion des Ressources Humaines",
            new Dictionary<string,string[]>
            {
                { "Consultant RH", new[]{ "Recrutement","Communication","Formation","Paie","Gestion RH"} },
                { "Responsable RH", new[]{ "Management","Recrutement","Paie","Leadership","Droit social"} },
                { "Consultant", new[]{ "Analyse","PowerPoint","Communication","Gestion de projet","Stratégie"} }
            }
        },

        {
            "Informatique, Data, Télécommunications & Digital",
            new Dictionary<string,string[]>
            {
                { "Développeur .NET", new[]{ "C#",".NET","SQL","Git","Docker"} },
                { "Développeur Front-End", new[]{ "React","TypeScript","HTML","CSS","JavaScript"} },
                { "Data Analyst", new[]{ "Python","SQL","Power BI","Excel","Statistiques"} },
                { "Data Scientist", new[]{ "Python","Machine Learning","Pandas","SQL","TensorFlow"} },
                { "Administrateur Système", new[]{ "Linux","Docker","Réseau","Cloud","Bash"} }
            }
        }
    };

            var niveaux = new[]
            {
        "Bac",
        "Bac+2",
        "Licence",
        "Master"
    };

            var candidatesCsv = new StringBuilder();

            candidatesCsv.AppendLine(
                "Id;Nom;NiveauEtude;Secteur;MetierVise;Localisation;Competences");

            for (int i = 1; i <= 100; i++)
            {
                var secteur = jobs.Keys.ElementAt(random.Next(jobs.Count));

                var metiers = jobs[secteur];

                var metier = metiers.Keys.ElementAt(random.Next(metiers.Count));

                var competences = metiers[metier]
                    .OrderBy(x => random.Next())
                    .Take(random.Next(3, 6));

                var niveau = niveaux[random.Next(niveaux.Length)];

                var ville = villes[random.Next(villes.Length)];

                var nom = $"{prenoms[random.Next(prenoms.Length)]} {noms[random.Next(noms.Length)]}";

                candidatesCsv.AppendLine(
                    $"CAN{i:D3};{nom};{niveau};{secteur};{metier};{ville};{string.Join(", ", competences)}");
            }

            File.WriteAllText(
                "Data/candidats.csv",
                candidatesCsv.ToString(),
                Encoding.UTF8);

            Console.WriteLine("100 candidats générés.");

            // À ajouter à la fin de la méthode GenerateFiles() dans DataGenerator.cs

            var groundTruthCsv = new StringBuilder();
            groundTruthCsv.AppendLine("candidate_id;job_id");

            // On charge les offres réelles pour créer un lien
            var offers = File.ReadAllLines("Data/offres.csv").Skip(1).Take(50).ToList();

            for (int i = 1; i <= 100; i++)
            {
                string candId = $"CAN{i:D3}";
                // On simule que pour chaque candidat, 2 à 3 offres prises au hasard dans le tas sont les "bonnes"
                // Dans un vrai projet, ce serait fait par un expert humain.
                for (int j = 0; j < 2; j++)
                {
                    var randomOfferLine = offers[random.Next(offers.Count)];
                    var offerId = randomOfferLine.Split(';')[0];
                    groundTruthCsv.AppendLine($"{candId};{offerId}");
                }
            }

            File.WriteAllText("Data/Appariement_Demandeurs_Offres.csv", groundTruthCsv.ToString(), Encoding.UTF8);
            Console.WriteLine("Fichier de vérité (Ground Truth) généré pour les tests.");
        }
    }
}