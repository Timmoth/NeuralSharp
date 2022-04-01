namespace Snake.Behaviours;

public record SnakeGameResult(int Score, int Moves)
{
    public float Fitness => (Score + 1) * (Score + 1) +
                            (float)Moves / SnakeGameConfig.LifeTime;
}