using starskycore.Helpers;

namespace starskytest.FakeCreateAn
{
	public class CreateAnExifToolUnix
	{
		private static readonly string ImageExifToolTarGzUnix =
			"H4sIAKfMwl4AA+3YzU/CMBgG8HZiRL1wMh5rjEeghX3E2zRg4sGYCAdvOmUIEVmiqBz3p9vZF0EZGJMZpz6/pHk" +
			"YdF1J846V47vgJiw3x/1uO4oGZaUq+/tVli0ppec44jVdk9okzYGqu650bFd5Skjl1pRiwsl4HqkeH0bBvZ5Kpx" +
			"8NK0/B8CkcdMKUfs+9MBwsGef9lxLfNNvMHaetf6gPR/owo2t8uv51e7r+Tl2vv6f7MSEzuv5S/3z9d3eqV/1h9a" +
			"G3EV73ItENbsMmrf7GT88Nvl9q/VcuKo3WRWsU3adVwpfpenBt+3391+xp/Svvw/2/JpWyc1b/ulu3u2ScX1r/bH" +
			"VrjVmMnQTX4rQlzgVJ3mPrutV0O9ItOR4nJ0x6+KVFQx6022f0cjw5C/Ipvf6zrP70+p+WirKltD88/3muV89Z/f" +
			"/R33+NHz52FGNJOReZSbaX3rVIbY41Ox6NAQAAAPnGTRQ3f3YaAJBDyf1BUPqUsUlOn1uUhZlzSpSC0qeMTXLqZ1" +
			"EWKIuUJUpB6VPGJummxWnzwenKnHYonHYhXFD6X/rKAP/GiolS8vvfZAv3/wDwh/FCo9U4ZG8bgvkOul3OvI7Z4ocA" +
			"y/xZuD1zrqD0KWOTeBAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAFjmBahNaPIAUAAA";

		public static readonly byte[] BytesUnix = Base64Helper.TryParse(ImageExifToolTarGzUnix);
	}
}
