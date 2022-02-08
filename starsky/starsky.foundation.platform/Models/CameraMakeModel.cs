namespace starsky.foundation.platform.Models
{
	public class CameraMakeModel
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
		
		public string Make { get; set; }
		public string Model { get; set; }
	}
}
