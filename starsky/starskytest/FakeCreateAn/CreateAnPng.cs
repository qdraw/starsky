using starskycore.Helpers;

namespace starskytest.FakeCreateAn
{
	public static class CreateAnPng
	{
		private static readonly string Base64pngString = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFc" +
		                                                 "SJAAAAE3RFWHQAAAATdEVYdFRpdGxlAG9yYW5nZS10" +
		                                                 "aQAAAAd0SU1FB9ICAQAAAFFeI4EAAAAfdEVYdAAAAB" +
		                                                 "90RVh0Y3JlYXRlLWRhdGUAMjAwMi0wMi0wMVQwMDow" +
		                                                 "AAAAH3RFWHQAAAAfdEVYdERlc2NyaXB0aW9uAGRlc2" +
		                                                 "NyaXB0aW9uLW9yYQAAAA1JREFUeAFj+O/LcAYABbQC" +
		                                                 "Gaiu2/kAAADwZVhJZk1NACoAAAAIAAYBGgAFAAAAAQ" +
		                                                 "AAAFYBGwAFAAAAAQAAAF4BKAADAAAAAQACAAABMgAC" +
		                                                 "AAAAFAAAAGYCEwADAAAAAQABAACHaQAEAAAAAQAAAH" +
		                                                 "oAAAAAAAAASAAAAAEAAABIAAAAATIwMDI6MDI6MDEg" +
		                                                 "MDA6MDA6MDAAAAaQAAAHAAAABDAyMzKQAwACAAAAFA" +
		                                                 "AAAMiQBAACAAAAFAAAANyRAQAHAAAABAECAwCgAAAH" +
		                                                 "AAAABDAxMDCgAQADAAAAAf//AAAAAAAAMjAwMjowMj" +
		                                                 "owMSAwMDowMDowMAAyMDAyOjAyOjAxIDAwOjAwOjAw" +
		                                                 "AKnQtrkAAAAmelRYdFJhdyBwcm9maWxlIHR5cGUgaX" +
		                                                 "B0YwAAeJyTYWJgYGJgBgAA4AAk0FaDXgAAExhpVFh0" +
		                                                 "WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldC" +
		                                                 "BiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVT" +
		                                                 "ek5UY3prYzlkIj8+Cjx4OnhtcG1ldGEgeG1sbnM6eD" +
		                                                 "0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IlhNUCBD" +
		                                                 "b3JlIDUuMS4yIj4KIDxyZGY6UkRGIHhtbG5zOnJkZj" +
		                                                 "0iaHR0cDovL3d3dy53My5vcmcvMTk5OS8wMi8yMi1y" +
		                                                 "ZGYtc3ludGF4LW5zIyI+CiAgPHJkZjpEZXNjcmlwdG" +
		                                                 "lvbiByZGY6YWJvdXQ9IiIKICAgIHhtbG5zOnBob3Rv" +
		                                                 "c2hvcD0iaHR0cDovL25zLmFkb2JlLmNvbS9waG90b3" +
		                                                 "Nob3AvMS4wLyIKICAgIHhtbG5zOmV4aWY9Imh0dHA6" +
		                                                 "Ly9ucy5hZG9iZS5jb20vZXhpZi8xLjAvIgogICAgeG" +
		                                                 "1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hh" +
		                                                 "cC8xLjAvIgogICAgeG1sbnM6cGhvdG9tZWNoYW5pYz" +
		                                                 "0iaHR0cDovL25zLmNhbWVyYWJpdHMuY29tL3Bob3Rv" +
		                                                 "bWVjaGFuaWMvMS4wLyIKICAgcGhvdG9zaG9wOkRhdG" +
		                                                 "VDcmVhdGVkPSIyMDIwLTAyLTI1VDE5OjE3OjM0KzAx" +
		                                                 "OjAwIgogICBleGlmOkdQU0xhdGl0dWRlPSI0NSwzMy" +
		                                                 "42MTVOIgogICBleGlmOkdQU0xvbmdpdHVkZT0iMTIy" +
		                                                 "LDM5LjY2NVciCiAgIHhtcDpMYWJlbD0iIgogICB4bX" +
		                                                 "A6UmF0aW5nPSIwIgogICBwaG90b21lY2hhbmljOkNv" +
		                                                 "bG9yQ2xhc3M9IjAiCiAgIHBob3RvbWVjaGFuaWM6VG" +
		                                                 "FnZ2VkPSJGYWxzZSIKICAgcGhvdG9tZWNoYW5pYzpQ" +
		                                                 "cmVmcz0iMDowOjA6LTAwMDAxIgogICBwaG90b21lY2" +
		                                                 "hhbmljOlBNVmVyc2lvbj0iUE01Ii8+CiA8L3JkZjpS" +
		                                                 "REY+CjwveDp4bXBtZXRhPgogICAgICAgICAgICAgIC" +
		                                                 "AgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC" +
		                                                 "AgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC" +
		                                                 "AgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKIC" +
		                                                 "AgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC" +
		                                                 "AgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgIC" +
		                                                 "AgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC" +
		                                                 "AgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC" +
		                                                 "AgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgIC" +
		                                                 "AgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC" +
		                                                 "AgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC" +
		                                                 "AgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCi" +
		                                                 "AgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIC" +
		                                                 "AgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKPD94cGFja2V0IGVuZD0idyI/Prk7QHgAAAAASUVORK5CYII=";

			
		public static readonly byte[] Bytes = Base64Helper.TryParse(Base64pngString);
	}
}
