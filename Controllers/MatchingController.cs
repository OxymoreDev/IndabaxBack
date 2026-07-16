using Microsoft.AspNetCore.Mvc;
using WebApplication1.Features.Services;
using WebApplication1.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MatchingController : ControllerBase
    {
        private readonly DataService _dataService;
        private readonly MatchEngine _matchEngine;

        public MatchingController(DataService dataService, MatchEngine matchEngine)
        {
            _dataService = dataService;
            _matchEngine = matchEngine;
        }

        [HttpGet("results")]
        public ActionResult<List<MatchResult>> GetResults([FromQuery] int k = 5)
        {
            var candidates = _dataService.GetCandidates();
            var offers = _dataService.GetOffers();
            // On passe k=5 pour avoir le Top 5 officiel
            var results = _matchEngine.GetTopKMatches(candidates, offers, k);
            return Ok(results);
        }

        

        [HttpGet("search")]
        public ActionResult<List<MatchResult>> Search([FromQuery] string query, [FromQuery] int k = 5)
        {
            // 1. On récupère TOUS les vrais candidats de ta base
            var candidates = _dataService.GetCandidates();

            // 2. On crée une "Offre fictive" basée sur la saisie de l'utilisateur
            var dummyOffer = new JobOffer
            {
                Id = "SEARCH_QUERY",
                Intitule = query,
                Secteur = query,
                Entreprise = "Besoin spécifique"
            };

            // 3. On demande au moteur : "Quels candidats (réels) correspondent à ce besoin ?"
            // On passe la liste de TOUS les candidats et seulement NOTRE offre fictive
            var results = _matchEngine.GetTopKMatches(candidates, new List<JobOffer> { dummyOffer }, k);

            return Ok(results);
        }


        [HttpGet("export-officiel")]
        public ActionResult<List<Recommendation>> ExportOfficiel()
        {
            var candidates = _dataService.GetCandidates();
            var offers = _dataService.GetOffers();

            // 1. On récupère le Top 5 via le moteur (Logiciel)
            var results = _matchEngine.GetTopKMatches(candidates, offers, 5);

            // 2. On convertit en classe Recommendation (Format Jury / Point G)
            var export = results.Select(r => new Recommendation
            {
                candidate_id = r.candidate_id,
                rank = r.rank,
                job_id = r.job_id,
                score = r.score // Déjà au format 0.xxxx
            }).ToList();

            return Ok(export);
        }


        [HttpGet("stats")]
        public IActionResult GetStats()
        {
            var candidates = _dataService.GetCandidates();
            var offers = _dataService.GetOffers();

            return Ok(new
            {
                totalCandidates = candidates.Count,
                totalOffers = offers.Count,
                // Remarque I : Secteurs les plus représentés
                sectorDistribution = offers.GroupBy(o => o.Secteur)
                                           .Select(g => new { name = g.Key, value = g.Count() })
                                           .OrderByDescending(x => x.value).Take(5).ToList(),
                // Remarque I : Métiers les plus demandés
                jobDistribution = offers.GroupBy(o => o.Intitule)
                                        .Select(g => new { name = g.Key, value = g.Count() })
                                        .OrderByDescending(x => x.value).Take(5).ToList(),
                cityDistribution = candidates.GroupBy(c => c.Localisation)
                                             .Select(g => new { name = g.Key, value = g.Count() }).ToList()
            });
        }


        [HttpGet("evaluate")]
        public IActionResult GetEvaluation()
        {
            var candidates = _dataService.GetCandidates();
            var offers = _dataService.GetOffers();

            // 1. On génère toutes nos prédictions
            var predictions = _matchEngine.GetTopKMatches(candidates, offers, 5);

            Console.WriteLine("----- PREMIERES PREDICTIONS -----");

            foreach (var p in predictions.Take(10))
            {
                Console.WriteLine(
                    $"{p.candidate_id} -> {p.job_id}");
            }

            // 2. On charge la vérité terrain (Le fichier fourni par l'ACPE)
            // IMPORTANT: Assure-toi que le fichier est dans ton dossier /Data
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Appariement_Demandeurs_Offres.csv");

            var evaluationService = new EvaluationService();
            var reality = evaluationService.LoadGroundTruth(path);

            // 3. On calcule les scores
            var metrics = evaluationService.Evaluate(predictions, reality);

            return Ok(metrics);
        }

        // GET: api/<MatchingController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<MatchingController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<MatchingController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<MatchingController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<MatchingController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
