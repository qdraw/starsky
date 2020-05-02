namespace starsky.foundation.platform.Interfaces
{
	public interface IConsole
	{
		void Write(string message);
		void WriteLine(string message);
		string ReadLine();
	}
}
