public class Stat
{
	public string title;
	public int value;

	public Stat(string title, int value = 0)
	{
		this.title = title;
		this.value = value;
	}

	public void Add(int gain)
	{
		value += gain;
	}
}
