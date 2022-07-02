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

internal record Height
{
    public double Amount;
    public string Name;

    public Height(string name, double height)
    {
        Name = name;
        Amount = height;
    }
}