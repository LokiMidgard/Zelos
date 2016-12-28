namespace Zelos.Scribe
{
    public enum ComparisionKind
    {
        UseHashIfFrozen = 1,
        UsePropertysExcludingSecret = 2 ,
        UsePropertysIncludingSecret = 4
    }
}