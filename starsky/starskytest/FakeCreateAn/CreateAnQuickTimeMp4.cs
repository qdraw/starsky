using starskycore.Helpers;

namespace starskytest.FakeCreateAn
{
	public class CreateAnQuickTimeMp4
	{
		private static readonly string Base64QuickTimeMp4String =
			"AAAAFGZ0eXBxdCAgAAAAAHF0ICAAAAAId2lkZQAAAdRtZGF0AAAAFQYFEQOH9E7NCkvcoZQ6w9SbFx8AgAAAABc" +
			"luAgCBf/4gNZ988Brh5pePU0VlUZf8AAAABolbgIAgX/x8gez754DXDyWyqMd8y5gRq6/gAAAABUGBREDh/ROzQ" +
			"pL3KGUOsPUmxcfAIAAAAAHIeEJET9dQAAAAAcheEJET11AAAAAFQYFEQOH9E7NCkvcoZQ6w9SbFx8AgAAAAAcBqI" +
			"FiE7SAAAAACAFqIFiE/7SAAAAAFQYFEQOH9E7NCkvcoZQ6w9SbFx8AgAAAAAch4hEQj/5gAAAAByF4hEQj/mAAAA" +
			"AVBgURA4f0Ts0KS9yhlDrD1JsXHwCAAAAABwGow2Inw4AAAAAHAWow2InDgAAAABUGBREDh/ROzQpL3KGUOsPUmx" +
			"cfAIAAAAAHIeMZEL/+YAAAAAcheMZEL/5gAAAAFQYFEQOH9E7NCkvcoZQ6w9SbFx8AgAAAAAcBqQViP7uAAAAAB" +
			"wFqQViPu4AAAAAVBgURA4f0Ts0KS9yhlDrD1JsXHwCAAAAAByHkIRCv/mAAAAAHIXkIRCv+YAAAABUGBREDh/RO" +
			"zQpL3KGUOsPUmxcfAIAAAAAHAalHYhG3gAAAAAgBalHYhH+3gAAABWVtb292AAAAbG12aGQAAAAA2qZOr9qmTq8" +
			"AABdwAAADhAABAAABAAAAAAAAAAAAAAAAAQAAAAAAAAAAAAAAAAAAAAEAAAAAAAAAAAAAAAAAAEAAAAAAAAAAAA" +
			"AAAAAAAAAAAAAAAAAAAAAAAAAAAAACAAADfXRyYWsAAABcdGtoZAAAAA/apk6v2qZOrwAAAAEAAAAAAAADhAAAA" +
			"AAAAAAAAAAAAAEAAAAAAQAAAAAAAAAAAAAAAAAAAAEAAAAAAAAAAAAAAAAAAEAAAAAAFAAAABQAAAAAAER0YXB0A" +
			"AAAFGNsZWYAAAAAABQAAAAUAAAAAAAUcHJvZgAAAAAAFAAAABQAAAAAABRlbm9mAAAAAAAUAAAAFAAAAAAAJGVk" +
			"dHMAAAAcZWxzdAAAAAAAAAABAAADhAAAAAAAAQAAAAACsW1kaWEAAAAgbWRoZAAAAADapk6v2qZOrwAAF3AAAAO" +
			"EVcQAAAAAADFoZGxyAAAAAG1obHJ2aWRlYXBwbAAAAAAAAAAAEENvcmUgTWVkaWEgVmlkZW8AAAJYbWluZgAAAB" +
			"R2bWhkAAAAAQBAgACAAIAAAAAAOGhkbHIAAAAAZGhscmFsaXNhcHBsAAAAAAAAAAAXQ29yZSBNZWRpYSBEYXRhI" +
			"EhhbmRsZXIAAAAkZGluZgAAABxkcmVmAAAAAAAAAAEAAAAMYWxpcwAAAAEAAAHgc3RibAAAALdzdHNkAAAAAAAA" +
			"AAEAAACnYXZjMQAAAAAAAAABAAAAAAAAAAAAAAIAAAACAAAUABQASAAAAEgAAAAAAAAAAQVILjI2NAAAAAAAAAA" +
			"AAAAAAAAAAAAAAAAAAAAAAAAAABj//wAAACthdmNDAU1ACv/hABQnTUAKqRkvPPgLUBAQGkwrXvfAQAEABCj+CY" +
			"gAAAASY29scm5jbGMAAQABAAEAAAAQcGFzcAAAAAEAAAABAAAAAAAAABhzdHRzAAAAAAAAAAEAAAAJAAAAZAAAA" +
			"FhjdHRzAAAAAAAAAAkAAAABAAAAAAAAAAEAAABkAAAAAf///5wAAAABAAAAZAAAAAH///+cAAAAAQAAAGQAAAAB" +
			"////nAAAAAEAAABkAAAAAf///5wAAAAgY3NsZwAAAAAAAABk////nAAAAGQAAAAAAAADhAAAABRzdHNzAAAAAAA" +
			"AAAEAAAABAAAAFXNkdHAAAAAAIBAYEBgQGBAYAAAAHHN0c2MAAAAAAAAAAQAAAAEAAAAJAAAAAQAAADhzdHN6AA" +
			"AAAAAAAAAAAAAJAAAAUgAAAC8AAAAwAAAALwAAAC8AAAAvAAAALwAAAC8AAAAwAAAAFHN0Y28AAAAAAAAAAQAAAC" +
			"QAAAF0bWV0YQAAACJoZGxyAAAAAAAAAABtZHRhAAAAAAAAAAAAAAAAAAAAAACda2V5cwAAAAAAAAAEAAAAIG1kdG" +
			"Fjb20uYXBwbGUucXVpY2t0aW1lLm1ha2UAAAAhbWR0YWNvbS5hcHBsZS5xdWlja3RpbWUubW9kZWwAAAAkbWR0YW" +
			"NvbS5hcHBsZS5xdWlja3RpbWUuc29mdHdhcmUAAAAobWR0YWNvbS5hcHBsZS5xdWlja3RpbWUuY3JlYXRpb25kYXR" +
			"lAAAArWlsc3QAAAAdAAAAAQAAABVkYXRhAAAAAU5MOYRBcHBsZQAAACYAAAACAAAAHmRhdGEAAAABTkw5hE1hY0J" +
			"vb2tQcm8xNSwxAAAAMgAAAAMAAAAqZGF0YQAAAAFOTDmETWFjIE9TIFggMTAuMTQuNiAoMThHNDAzMikAAAAwAAA" +
			"ABAAAAChkYXRhAAAAAU5MOYQyMDIwLTAzLTI5VDE1OjA5OjM5KzAyMDA=";
		
		public static readonly byte[] Bytes = Base64Helper.TryParse(Base64QuickTimeMp4String);

	}
}
