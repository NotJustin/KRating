namespace KRating;

public partial class Rating
{
    public override string ModuleName => "KRating";
    public override string ModuleAuthor => "ff";
    public override string ModuleVersion => "0.0.3";
    public const int ConfigVersion = 3;
    public bool tableExists = false;
    public List<Player> players = new();
    public string DatabaseConnectionString = string.Empty;
    public float RatingDistance;
}
