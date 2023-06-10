namespace BadChessBot;

public enum Faction
{
    White, Black
}

public static class FactionUtil
{
    public static Faction OppositeFaction(this Faction faction)
    {
        return faction == Faction.White ? Faction.Black : Faction.White;
    }
}
