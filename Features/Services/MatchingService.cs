namespace WebApplication1.Features.Services
{
    public class MatchingService
    {
        public double CalculateScore(float[] vectorA, float[] vectorB)
        {
            // La formule mathématique pour voir si deux vecteurs pointent dans la même direction
            float dotProduct = 0;
            float magnitudeA = 0;
            float magnitudeB = 0;
            for (int i = 0; i < vectorA.Length; i++)
            {
                dotProduct += vectorA[i] * vectorB[i];
                magnitudeA += vectorA[i] * vectorA[i];
                magnitudeB += vectorB[i] * vectorB[i];
            }
            return dotProduct / (Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB));
        }
    }
}