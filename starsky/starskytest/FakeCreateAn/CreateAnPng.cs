using starskycore.Helpers;

namespace starskytest.FakeCreateAn
{
	public static class CreateAnPng
	{
		private static readonly string Base64pngString = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFc" +
		                                                 "SJAAAG+mlUWHRYTUw6Y29tLmFkb2JlLnhtcAAAAAAA" +
		                                                 "PD94cGFja2V0IGJlZ2luPSfvu78nIGlkPSdXNU0wTX" +
		                                                 "BDZWhpSHpyZVN6TlRjemtjOWQnPz4KPHg6eG1wbWV0Y" +
		                                                 "SB4bWxuczp4PSdhZG9iZTpuczptZXRhLycgeDp4bXB0" +
		                                                 "az0nSW1hZ2U6OkV4aWZUb29sIDExLjcwJz4KPHJkZjp" +
		                                                 "SREYgeG1sbnM6cmRmPSdodHRwOi8vd3d3LnczLm9yZy8" +
		                                                 "xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjJz4KCiA8cmR" +
		                                                 "mOkRlc2NyaXB0aW9uIHJkZjphYm91dD0nJwogIHhtbG5" +
		                                                 "zOmRjPSdodHRwOi8vcHVybC5vcmcvZGMvZWxlbWVudHM" +
		                                                 "vMS4xLyc+CiAgPGRjOmRlc2NyaXB0aW9uPgogICA8cmR" +
		                                                 "mOkFsdD4KICAgIDxyZGY6bGkgeG1sOmxhbmc9J3gtZG" +
		                                                 "VmYXVsdCc+RGVzY3JpcHRpb248L3JkZjpsaT4KICAg" +
		                                                 "PC9yZGY6QWx0PgogIDwvZGM6ZGVzY3JpcHRpb24+Ci" +
		                                                 "AgPGRjOnN1YmplY3Q+CiAgIDxyZGY6QmFnPgogICAg" +
		                                                 "PHJkZjpsaT50YWdzIDwvcmRmOmxpPgogICA8L3JkZjp" +
		                                                 "CYWc+CiAgPC9kYzpzdWJqZWN0PgogIDxkYzp0aXRsZ" +
		                                                 "T4KICAgPHJkZjpBbHQ+CiAgICA8cmRmOmxpIHhtbDps" +
		                                                 "YW5nPSd4LWRlZmF1bHQnPnRpdGxlPC9yZGY6bGk+CiA" +
		                                                 "gIDwvcmRmOkFsdD4KICA8L2RjOnRpdGxlPgogPC9yZG" +
		                                                 "Y6RGVzY3JpcHRpb24+CgogPHJkZjpEZXNjcmlwdGlvb" +
		                                                 "iByZGY6YWJvdXQ9JycKICB4bWxuczpleGlmPSdodHRw" +
		                                                 "Oi8vbnMuYWRvYmUuY29tL2V4aWYvMS4wLyc+CiAgPGV" +
		                                                 "4aWY6RXhwb3N1cmVUaW1lPjEvMzA8L2V4aWY6RXhwb3" +
		                                                 "N1cmVUaW1lPgogIDxleGlmOkZvY2FsTGVuZ3RoPjgwL" +
		                                                 "zE8L2V4aWY6Rm9jYWxMZW5ndGg+CiAgPGV4aWY6R1BT" +
		                                                 "QWx0aXR1ZGU+MTAvMTwvZXhpZjpHUFNBbHRpdHVkZT4" +
		                                                 "KICA8ZXhpZjpHUFNBbHRpdHVkZVJlZj4wPC9leGlmOk" +
		                                                 "dQU0FsdGl0dWRlUmVmPgogIDxleGlmOkdQU0xhdGl0d" +
		                                                 "WRlPjM1LDIuMjhOPC9leGlmOkdQU0xhdGl0dWRlPgog" +
		                                                 "IDxleGlmOkdQU0xvbmdpdHVkZT44MSwzLjEyVzwvZXh" +
		                                                 "pZjpHUFNMb25naXR1ZGU+CiAgPGV4aWY6SVNPU3BlZW" +
		                                                 "RSYXRpbmdzPgogICA8cmRmOlNlcT4KICAgIDxyZGY6b" +
		                                                 "Gk+MTAwPC9yZGY6bGk+CiAgICA8cmRmOmxpPjEwMDwv" +
		                                                 "cmRmOmxpPgogICA8L3JkZjpTZXE+CiAgPC9leGlmOkl" +
		                                                 "TT1NwZWVkUmF0aW5ncz4KIDwvcmRmOkRlc2NyaXB0aW" +
		                                                 "9uPgoKIDxyZGY6RGVzY3JpcHRpb24gcmRmOmFib3V0P" +
		                                                 "ScnCiAgeG1sbnM6cGhvdG9tZWNoYW5pYz0naHR0cDov" +
		                                                 "L25zLmNhbWVyYWJpdHMuY29tL3Bob3RvbWVjaGFuaWM" +
		                                                 "vMS4wLyc+CiAgPHBob3RvbWVjaGFuaWM6Q29sb3JDbG" +
		                                                 "Fzcz40PC9waG90b21lY2hhbmljOkNvbG9yQ2xhc3M+C" +
		                                                 "iA8L3JkZjpEZXNjcmlwdGlvbj4KCiA8cmRmOkRlc2Ny" +
		                                                 "aXB0aW9uIHJkZjphYm91dD0nJwogIHhtbG5zOnBob3R" +
		                                                 "vc2hvcD0naHR0cDovL25zLmFkb2JlLmNvbS9waG90b3" +
		                                                 "Nob3AvMS4wLyc+CiAgPHBob3Rvc2hvcDpDaXR5PkNpd" +
		                                                 "Hk8L3Bob3Rvc2hvcDpDaXR5PgogIDxwaG90b3Nob3A6" +
		                                                 "Q291bnRyeT5Db3VudHJ5PC9waG90b3Nob3A6Q291bnR" +
		                                                 "yeT4KICA8cGhvdG9zaG9wOkRhdGVDcmVhdGVkPjIwMj" +
		                                                 "ItMDYtMTJUMTA6NDU6MzE8L3Bob3Rvc2hvcDpEYXRlQ" +
		                                                 "3JlYXRlZD4KICA8cGhvdG9zaG9wOlN0YXRlPlN0YXRl" +
		                                                 "PC9waG90b3Nob3A6U3RhdGU+CiA8L3JkZjpEZXNjcml" +
		                                                 "wdGlvbj4KCiA8cmRmOkRlc2NyaXB0aW9uIHJkZjphYm" +
		                                                 "91dD0nJwogIHhtbG5zOnhtcD0naHR0cDovL25zLmFkb" +
		                                                 "2JlLmNvbS94YXAvMS4wLyc+CiAgPHhtcDpMYWJlbD5T" +
		                                                 "dXBlcmlvciBBbHQ8L3htcDpMYWJlbD4KIDwvcmRmOkR" +
		                                                 "lc2NyaXB0aW9uPgo8L3JkZjpSREY+CjwveDp4bXBtZX" +
		                                                 "RhPgo8P3hwYWNrZXQgZW5kPSdyJz8+Ek36GQAAAKZ6V" +
		                                                 "Fh0UmF3IHByb2ZpbGUgdHlwZSBpcHRjAAB4nD1NOQ4C" +
		                                                 "MQzs84p9QhJfSU1FR8EHlk0iISGB+H/B2Cthy4k9Hs+" +
		                                                 "k6+1+2T7f93q+ZtoiekvUuHLnkRn5D5Fy5Co7WmbSbm" +
		                                                 "w9kBVLMtaCCpYKMEugLROdoNaTHEr+sk+le+eHakaOW" +
		                                                 "APyYMgYKVmFTTZO2nXpDAFXFpzAX4/TbQx3o0w7Mcp/" +
		                                                 "AOkHaKoxBEOtNm4AAAAXdEVYdERlc2NyaXB0aW9uAER" +
		                                                 "lc2NyaXB0aW9uCg7OWgAAAAt0RVh0VGl0bGUAdGl0bG" +
		                                                 "URKDEmAAAAH3RFWHRjcmVhdGUtZGF0ZQAyMDIyLTA2L" +
		                                                 "TEyVDEwOjQ1OjMxQevrAgAAAAd0SU1FB+YGDAotH2nq" +
		                                                 "yusAAAAQSURBVHjaAQUA+v8A+fr6/wnKA+1Bv9MtAAA" +
		                                                 "BwmVYSWZNTQAqAAAACAAHARoABQAAAAEAAABiARsABQ" +
		                                                 "AAAAEAAABqASgAAwAAAAEAAgAAATIAAgAAABQAAAByA" +
		                                                 "hMAAwAAAAEAAQAAh2kABAAAAAEAAACGiCUABAAAAAEA" +
		                                                 "AAEwAAAAAAAAAEgAAAABAAAASAAAAAEyMDIyOjA2OjE" +
		                                                 "yIDEwOjQ1OjMxAAAJgpoABQAAAAEAAAD4iCcAAwAAAA" +
		                                                 "EAZAAAkAAABwAAAAQwMjMykAMAAgAAABQAAAEAkAQAA" +
		                                                 "gAAABQAAAEUkQEABwAAAAQBAgMAkgoABQAAAAEAAAEo" +
		                                                 "oAAABwAAAAQwMTAwoAEAAwAAAAH//wAAAAAAAAAAAAE" +
		                                                 "AAAAeMjAyMjowNjoxMiAxMDo0NTozMQAyMDIyOjA2Oj" +
		                                                 "EyIDEwOjQ1OjMxAAAAAFAAAAABAAcAAAABAAAABAIDA" +
		                                                 "AAAAQACAAAAAk4AAAAAAgAFAAAAAwAAAYoAAwACAAAA" +
		                                                 "AlcAAAAABAAFAAAAAwAAAaIABQABAAAAAQAAAAAABgA" +
		                                                 "FAAAAAQAAAboAAAAAAAAAIwAAAAEAAAACAAAAAQAAAF" +
		                                                 "QAAAAFAAAAUQAAAAEAAAADAAAAAQAAACQAAAAFAAAAC" +
		                                                 "gAAAAHgYX43AAAAAElFTkSuQmCC";
			
		public static readonly byte[] Bytes = Base64Helper.TryParse(Base64pngString);
	}
}
