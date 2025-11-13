namespace starskyiocli;

public class RequestIoModel
{
	public string Method { get; set; }
	public string Path { get; set; }
	public Dictionary<string, string> Parameters { get; set; } = new();
}
