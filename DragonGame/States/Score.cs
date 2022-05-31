namespace DuckDuckJump.States;

internal record Score
{
    public string Name;
    public int Time;

    public Score(string name, int time)
    {
        Name = name;
        Time = time;
    }
}