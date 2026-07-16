using Scalar.AspNetCore;
using WebApplication1.Features.Services;
using WebApplication1.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 1. Enregistre tes services dans le conteneur de dépendances (Injection de dépendances)
// Le prof explique : Cela permet d'utiliser ces services partout dans ton projet
builder.Services.AddSingleton<DataService>();
builder.Services.AddSingleton<MatchEngine>();
builder.Services.AddSingleton<EvaluationService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(config =>
        config.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();



using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dataService = services.GetRequiredService<DataService>();

    var candidates = dataService.GetCandidates();
    var offers = dataService.GetOffers();

}
//==========================================
//BLOC DE TEST(À supprimer une fois validé)
//==========================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        Console.WriteLine("=== DÉMARRAGE DU TEST DE MATCHING ===");

        var dataService = services.GetRequiredService<DataService>();
        var matchEngine = services.GetRequiredService<MatchEngine>();


        // Chargement des données
        var candidates = dataService.GetCandidates();
        var offers = dataService.GetOffers();

        
        

        Console.WriteLine($"Données chargées : {candidates.Count} candidats et {offers.Count} offres.");

        // 3. Calcul du TOP-5 (La nouvelle méthode !)
        var topMatches = matchEngine.GetTopKMatches(candidates, offers, 5);


        // 4. Affichage des 2 premiers candidats pour vérification
        var sample = topMatches.Take(10); // 2 candidats x 5 offres = 10 lignes
        foreach (var m in sample)
        {
            Console.WriteLine($"[Rang {m.rank}] Cand: {m.candidate_name} ({m.candidate_id}) -> Job: {m.job_title} | Score: {m.score}");
            Console.WriteLine($"   Communs: {string.Join(", ", m.common_skills)}");
            Console.WriteLine($"   Manquants: {string.Join(", ", m.missing_skills)}");
        }


        Console.WriteLine("=== TEST TERMINÉ AVEC SUCCÈS ===");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erreur pendant le test : {ex.Message}");
        // Si ça plante ici, vérifie que tes fichiers CSV sont bien dans /Data
        // et que les noms des colonnes correspondent à tes modèles !
    }
}
 //==========================================


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseCors();


app.UseAuthorization();

app.MapControllers();

app.Run();
