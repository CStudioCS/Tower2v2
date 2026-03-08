public class TeamStats
{
	public PlayerTeam.Team team;
	public Stat distanceTravelled = new Stat("Distance travelled");
	public Stat woodCut = new Stat("Wood cut");
	public Stat itemsStolen = new Stat("Items stolen");
	public Stat[] stats;
	
	public TeamStats(PlayerTeam.Team team)
	{
		this.team = team;
		stats = new Stat[]
		{
			distanceTravelled,
			woodCut,
			itemsStolen
		};
	}
}