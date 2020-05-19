using starskycore.Helpers;

namespace starskytest.FakeCreateAn
{
	public static class CreateAnExifToolTarGz
	{
		private static readonly string ImageExifToolTarGzUnix =
			"H4sIACE9w14AA+3UPU7DMBQHcIOEEJ2YGJiMOje1ncZRJRYGkDogEM3ABDKNQyvSpCJpYeIsHKF34DLMiAPg0Fa" +
			"lKCoZwvf/Jz05thz7RS92q68udW3/thd4cRzWOLeazTopF2PMdRz62spJa8zaSYfbUjJpS5sJyrgUzCHUKTmPXM" +
			"MkVdcmFb8XR9ZIRSMd+jpn3k1X63DJOosfRT8p29K18upvnWszkJqBUvbIq79ozOsv5Jv6S0mZYIJLQlkpu3+ga" +
			"P3NtCBYss4vrT9Z21onq4Qcqg49atNTOpWNkQ0TwsSdiaw/LrbknuedTB+zN+5NbL6bsjIf3+7EfUsNBqG2QpWk" +
			"w0T7vkp19bidTXzafTjL2sfn8Et+h/8m9/yXevoL3P+2s3j+ueu6jR92/v/o/V/dqV/0onrSrehON6YH6krT2d9" +
			"Q+e7kAAAAAAAAAAAAAAAAAAAAoJAXspDxGwAoAAA=";

		public static readonly byte[] Bytes = Base64Helper.TryParse(ImageExifToolTarGzUnix);
		
		public static readonly string Sha1 = "b386a6849ed5f911085cc56f37d20f127162b21c";

	}
}
