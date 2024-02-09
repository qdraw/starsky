namespace starsky.foundation.platform.Models
{
	public sealed class CameraMakeModel
	{
		public CameraMakeModel()
		{
			// nothing here
		}

		public CameraMakeModel(string make, string model)
		{
			Make = make;
			Model = model;
		}

		public string Make { get; set; } = string.Empty;
		public string Model { get; set; } = string.Empty;
	}
}
