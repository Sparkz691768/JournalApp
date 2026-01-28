namespace JournalApp.Models;

public enum MoodCategory { Positive, Neutral, Negative }

public enum MoodName
{
    // Positive
    Happy, Excited, Relaxed, Grateful, Confident,
    // Neutral
    Calm, Thoughtful, Curious, Nostalgic, Bored,
    // Negative
    Sad, Angry, Stressed, Lonely, Anxious
}

public static class MoodMap
{
    public static MoodCategory GetCategory(MoodName mood) => mood switch
    {
        MoodName.Happy or MoodName.Excited or MoodName.Relaxed or MoodName.Grateful or MoodName.Confident
            => MoodCategory.Positive,
        MoodName.Calm or MoodName.Thoughtful or MoodName.Curious or MoodName.Nostalgic or MoodName.Bored
            => MoodCategory.Neutral,
        _ => MoodCategory.Negative
    };
}
