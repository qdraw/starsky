using System;
using System.IO;
using System.Reflection;
using starskycore.Helpers;

namespace starskytest.FakeCreateAn
{
    public class CreateAnGpx
    {

        public readonly string FullFileGpxPath = 
            Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + Path.DirectorySeparatorChar + FileNameGpx;
        public string FileName => FileNameGpx;
        private const string FileNameGpx = "zz__test.gpx";

        public readonly string BasePath =
            Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + Path.DirectorySeparatorChar;
        
	    private static readonly string Base64GpxString = "CjxncHggeG1sbnM9Imh0dHA6Ly93d3cudG9wb2dyYWZpeC5jb20vR1BYLzEvMSIg" +
                                "eG1sbnM6Z3B4eD0iaHR0cDovL3d3dy5nYXJtaW4uY29tL3htbHNjaGVtYXMvR3B4" +
                                "RXh0ZW5zaW9ucy92MyIgeG1sbnM6Z3B4dHB4PSJodHRwOi8vd3d3OC5nYXJtaW4u" +
                                "Y29tL3htbHNjaGVtYXMvVHJhY2tQb2ludEV4dGVuc2lvbnYyLnhzZCIgeG1sbnM6" +
                                "dHJhaWxzaW89Imh0dHA6Ly90cmFpbHMuaW8vR1BYLzEvMCIgeG1sbnM6eHNpPSJo" +
                                "dHRwOi8vd3d3LnczLm9yZy8yMDAxL1hNTFNjaGVtYS1pbnN0YW5jZSIgdmVyc2lv" +
                                "bj0iMS4xIiBjcmVhdG9yPSJBZHplIC0gaHR0cDovL2tvYm90c3cuY29tL2FwcHMv" +
                                "YWR6ZSIgeHNpOnNjaGVtYUxvY2F0aW9uPSJodHRwOi8vd3d3LnRvcG9ncmFmaXgu" +
                                "Y29tL0dQWC8xLzEgaHR0cDovL3d3dy50b3BvZ3JhZml4LmNvbS9HUFgvMS8xL2dw" +
                                "eC54c2QgaHR0cDovL3d3dy5nYXJtaW4uY29tL3htbHNjaGVtYXMvR3B4RXh0ZW5z" +
                                "aW9ucy92MyBodHRwOi8vd3d3Lmdhcm1pbi5jb20veG1sc2NoZW1hcy9HcHhFeHRl" +
                                "bnNpb25zdjMueHNkIGh0dHA6Ly90cmFpbHMuaW8vR1BYLzEvMCBodHRwczovL3Ry" +
                                "YWlscy5pby9HUFgvMS8wL3RyYWlsc18xLjAueHNkIj4KICAgIDxtZXRhZGF0YT4K" +
                                "ICAgICAgICA8bmFtZT5VbnRpdGxlZCBEb2N1bWVudDwvbmFtZT4KICAgICAgICA8" +
                                "dGltZT4yMDE4LTA5LTExVDE5OjQxOjEwLjQ4Mlo8L3RpbWU+CiAgICA8L21ldGFk" +
                                "YXRhPgogICAgPHRyaz4KICAgICAgICA8bmFtZT5fMjAxODA5MDUtZmlldHNlbi1v" +
                                "c3M8L25hbWU+CiAgICAgICAgPGV4dGVuc2lvbnM+CiAgICAgICAgICAgIDx0cmFp" +
                                "bHNpbzpUcmFja0V4dGVuc2lvbj4KICAgICAgICAgICAgICAgIDx0cmFpbHNpbzph" +
                                "Y3Rpdml0eT5iaWtpbmc8L3RyYWlsc2lvOmFjdGl2aXR5PgogICAgICAgICAgICA8" +
                                "L3RyYWlsc2lvOlRyYWNrRXh0ZW5zaW9uPgogICAgICAgICAgICA8Z3B4eDpUcmFj" +
                                "a0V4dGVuc2lvbj4KICAgICAgICAgICAgICAgIDxncHh4OkRpc3BsYXlDb2xvcj5E" +
                                "YXJrUmVkPC9ncHh4OkRpc3BsYXlDb2xvcj4KICAgICAgICAgICAgPC9ncHh4OlRy" +
                                "YWNrRXh0ZW5zaW9uPgogICAgICAgIDwvZXh0ZW5zaW9ucz4KICAgICAgICA8dHJr" +
                                "c2VnPgogICAgICAgICAgICA8dHJrcHQgbG9uPSI1LjQ4NTk0MSIgbGF0PSI1MS44" +
                                "MDkzNjAiPgogICAgICAgICAgICAgICAgPGVsZT43LjI2MzAwMDwvZWxlPgogICAg" +
                                "ICAgICAgICAgICAgPHRpbWU+MjAxOC0wOS0wNVQxNzozMTo1M1o8L3RpbWU+CiAg" +
                                "ICAgICAgICAgICAgICA8ZXh0ZW5zaW9ucz4KICAgICAgICAgICAgICAgICAgICA8" +
                                "Z3B4dHB4OlRyYWNrUG9pbnRFeHRlbnNpb24+CiAgICAgICAgICAgICAgICAgICAg" +
                                "ICAgIDxncHh0cHg6c3BlZWQ+NS40OTwvZ3B4dHB4OnNwZWVkPgogICAgICAgICAg" +
                                "ICAgICAgICAgICAgICA8Z3B4dHB4OmNvdXJzZT4xODkuNDk8L2dweHRweDpjb3Vy" +
                                "c2U+CiAgICAgICAgICAgICAgICAgICAgPC9ncHh0cHg6VHJhY2tQb2ludEV4dGVu" +
                                "c2lvbj4KICAgICAgICAgICAgICAgICAgICA8dHJhaWxzaW86VHJhY2tQb2ludEV4" +
                                "dGVuc2lvbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHRyYWlsc2lvOmhhY2M+" +
                                "NS4wMDwvdHJhaWxzaW86aGFjYz4KICAgICAgICAgICAgICAgICAgICAgICAgPHRy" +
                                "YWlsc2lvOnZhY2M+NC4wMDwvdHJhaWxzaW86dmFjYz4KICAgICAgICAgICAgICAg" +
                                "ICAgICAgICAgPHRyYWlsc2lvOnN0ZXBzPjU1MTE8L3RyYWlsc2lvOnN0ZXBzPgog" +
                                "ICAgICAgICAgICAgICAgICAgIDwvdHJhaWxzaW86VHJhY2tQb2ludEV4dGVuc2lv" +
                                "bj4KICAgICAgICAgICAgICAgIDwvZXh0ZW5zaW9ucz4KICAgICAgICAgICAgPC90" +
                                "cmtwdD4KICAgICAgICAgICAgPHRya3B0IGxvbj0iNS40ODU3MjQiIGxhdD0iNTEu" +
                                "ODA3OTY4Ij4KICAgICAgICAgICAgICAgIDxlbGU+Ny43NzIwMDA8L2VsZT4KICAg" +
                                "ICAgICAgICAgICAgIDx0aW1lPjIwMTgtMDktMDVUMTc6MzI6MjFaPC90aW1lPgog" +
                                "ICAgICAgICAgICAgICAgPGV4dGVuc2lvbnM+CiAgICAgICAgICAgICAgICAgICAg" +
                                "PGdweHRweDpUcmFja1BvaW50RXh0ZW5zaW9uPgogICAgICAgICAgICAgICAgICAg" +
                                "ICAgICA8Z3B4dHB4OnNwZWVkPjQuNTk8L2dweHRweDpzcGVlZD4KICAgICAgICAg" +
                                "ICAgICAgICAgICAgICAgPGdweHRweDpjb3Vyc2U+MTg1LjYyPC9ncHh0cHg6Y291" +
                                "cnNlPgogICAgICAgICAgICAgICAgICAgIDwvZ3B4dHB4OlRyYWNrUG9pbnRFeHRl" +
                                "bnNpb24+CiAgICAgICAgICAgICAgICAgICAgPHRyYWlsc2lvOlRyYWNrUG9pbnRF" +
                                "eHRlbnNpb24+CiAgICAgICAgICAgICAgICAgICAgICAgIDx0cmFpbHNpbzpoYWNj" +
                                "PjUuMDA8L3RyYWlsc2lvOmhhY2M+CiAgICAgICAgICAgICAgICAgICAgICAgIDx0" +
                                "cmFpbHNpbzp2YWNjPjQuMDA8L3RyYWlsc2lvOnZhY2M+CiAgICAgICAgICAgICAg" +
                                "ICAgICAgICAgIDx0cmFpbHNpbzpzdGVwcz41NTUyPC90cmFpbHNpbzpzdGVwcz4K" +
                                "ICAgICAgICAgICAgICAgICAgICA8L3RyYWlsc2lvOlRyYWNrUG9pbnRFeHRlbnNp" +
                                "b24+CiAgICAgICAgICAgICAgICA8L2V4dGVuc2lvbnM+CiAgICAgICAgICAgIDwv" +
                                "dHJrcHQ+CiAgICAgICAgICAgIDx0cmtwdCBsb249IjUuNDg1NjMxIiBsYXQ9IjUx" +
                                "LjgwNzAxOSI+CiAgICAgICAgICAgICAgICA8ZWxlPjkuOTU3MDAwPC9lbGU+CiAg" +
                                "ICAgICAgICAgICAgICA8dGltZT4yMDE4LTA5LTA1VDE3OjMyOjQzWjwvdGltZT4K" +
                                "ICAgICAgICAgICAgICAgIDxleHRlbnNpb25zPgogICAgICAgICAgICAgICAgICAg" +
                                "IDxncHh0cHg6VHJhY2tQb2ludEV4dGVuc2lvbj4KICAgICAgICAgICAgICAgICAg" +
                                "ICAgICAgPGdweHRweDpzcGVlZD40LjYzPC9ncHh0cHg6c3BlZWQ+CiAgICAgICAg" +
                                "ICAgICAgICAgICAgICAgIDxncHh0cHg6Y291cnNlPjE4MC43MDwvZ3B4dHB4OmNv" +
                                "dXJzZT4KICAgICAgICAgICAgICAgICAgICA8L2dweHRweDpUcmFja1BvaW50RXh0" +
                                "ZW5zaW9uPgogICAgICAgICAgICAgICAgICAgIDx0cmFpbHNpbzpUcmFja1BvaW50" +
                                "RXh0ZW5zaW9uPgogICAgICAgICAgICAgICAgICAgICAgICA8dHJhaWxzaW86aGFj" +
                                "Yz41LjAwPC90cmFpbHNpbzpoYWNjPgogICAgICAgICAgICAgICAgICAgICAgICA8" +
                                "dHJhaWxzaW86dmFjYz4zLjAwPC90cmFpbHNpbzp2YWNjPgogICAgICAgICAgICAg" +
                                "ICAgICAgICAgICA8dHJhaWxzaW86c3RlcHM+NTU1NjwvdHJhaWxzaW86c3RlcHM+" +
                                "CiAgICAgICAgICAgICAgICAgICAgPC90cmFpbHNpbzpUcmFja1BvaW50RXh0ZW5z" +
                                "aW9uPgogICAgICAgICAgICAgICAgPC9leHRlbnNpb25zPgogICAgICAgICAgICA8" +
                                "L3Rya3B0PgogICAgICAgICAgICA8dHJrcHQgbG9uPSI1LjQ4NTYxMCIgbGF0PSI1" +
                                "MS44MDY3MDIiPgogICAgICAgICAgICAgICAgPGVsZT4xMC44MDgwMDA8L2VsZT4K" +
                                "ICAgICAgICAgICAgICAgIDx0aW1lPjIwMTgtMDktMDVUMTc6MzI6NTBaPC90aW1l" +
                                "PgogICAgICAgICAgICAgICAgPGV4dGVuc2lvbnM+CiAgICAgICAgICAgICAgICAg" +
                                "ICAgPGdweHRweDpUcmFja1BvaW50RXh0ZW5zaW9uPgogICAgICAgICAgICAgICAg" +
                                "ICAgICAgICA8Z3B4dHB4OnNwZWVkPjUuNjQ8L2dweHRweDpzcGVlZD4KICAgICAg" +
                                "ICAgICAgICAgICAgICAgICAgPGdweHRweDpjb3Vyc2U+MTg2LjMzPC9ncHh0cHg6" +
                                "Y291cnNlPgogICAgICAgICAgICAgICAgICAgIDwvZ3B4dHB4OlRyYWNrUG9pbnRF" +
                                "eHRlbnNpb24+CiAgICAgICAgICAgICAgICAgICAgPHRyYWlsc2lvOlRyYWNrUG9p" +
                                "bnRFeHRlbnNpb24+CiAgICAgICAgICAgICAgICAgICAgICAgIDx0cmFpbHNpbzpo" +
                                "YWNjPjUuMDA8L3RyYWlsc2lvOmhhY2M+CiAgICAgICAgICAgICAgICAgICAgICAg" +
                                "IDx0cmFpbHNpbzp2YWNjPjMuMDA8L3RyYWlsc2lvOnZhY2M+CiAgICAgICAgICAg" +
                                "ICAgICAgICAgICAgIDx0cmFpbHNpbzpzdGVwcz41NTY2PC90cmFpbHNpbzpzdGVw" +
                                "cz4KICAgICAgICAgICAgICAgICAgICA8L3RyYWlsc2lvOlRyYWNrUG9pbnRFeHRl" +
                                "bnNpb24+CiAgICAgICAgICAgICAgICA8L2V4dGVuc2lvbnM+CiAgICAgICAgICAg" +
                                "IDwvdHJrcHQ+CiAgICAgICAgICAgIDx0cmtwdCBsb249IjUuNDg1NjMzIiBsYXQ9" +
                                "IjUxLjgwNTY2MyI+CiAgICAgICAgICAgICAgICA8ZWxlPjEwLjg2NjAwMDwvZWxl" +
                                "PgogICAgICAgICAgICAgICAgPHRpbWU+MjAxOC0wOS0wNVQxNzozMzoxMVo8L3Rp" +
                                "bWU+CiAgICAgICAgICAgICAgICA8ZXh0ZW5zaW9ucz4KICAgICAgICAgICAgICAg" +
                                "ICAgICA8Z3B4dHB4OlRyYWNrUG9pbnRFeHRlbnNpb24+CiAgICAgICAgICAgICAg" +
                                "ICAgICAgICAgIDxncHh0cHg6c3BlZWQ+NS41ODwvZ3B4dHB4OnNwZWVkPgogICAg" +
                                "ICAgICAgICAgICAgICAgICAgICA8Z3B4dHB4OmNvdXJzZT4xNjkuMTA8L2dweHRw" +
                                "eDpjb3Vyc2U+CiAgICAgICAgICAgICAgICAgICAgPC9ncHh0cHg6VHJhY2tQb2lu" +
                                "dEV4dGVuc2lvbj4KICAgICAgICAgICAgICAgICAgICA8dHJhaWxzaW86VHJhY2tQ" +
                                "b2ludEV4dGVuc2lvbj4KICAgICAgICAgICAgICAgICAgICAgICAgPHRyYWlsc2lv" +
                                "OmhhY2M+NS4wMDwvdHJhaWxzaW86aGFjYz4KICAgICAgICAgICAgICAgICAgICAg" +
                                "ICAgPHRyYWlsc2lvOnZhY2M+My4wMDwvdHJhaWxzaW86dmFjYz4KICAgICAgICAg" +
                                "ICAgICAgICAgICAgICAgPHRyYWlsc2lvOnN0ZXBzPjU2MDc8L3RyYWlsc2lvOnN0" +
                                "ZXBzPgogICAgICAgICAgICAgICAgICAgIDwvdHJhaWxzaW86VHJhY2tQb2ludEV4" +
                                "dGVuc2lvbj4KICAgICAgICAgICAgICAgIDwvZXh0ZW5zaW9ucz4KICAgICAgICAg" +
                                "ICAgPC90cmtwdD4KICAgICAgICAgICAgPHRya3B0IGxvbj0iNS40ODU3MzgiIGxh" +
                                "dD0iNTEuODA1NTAwIj4KICAgICAgICAgICAgICAgIDxlbGU+OS4yNDIwMDA8L2Vs" +
                                "ZT4KICAgICAgICAgICAgICAgIDx0aW1lPjIwMTgtMDktMDVUMTc6MzM6MTVaPC90" +
                                "aW1lPgogICAgICAgICAgICAgICAgPGV4dGVuc2lvbnM+CiAgICAgICAgICAgICAg" +
                                "ICAgICAgPGdweHRweDpUcmFja1BvaW50RXh0ZW5zaW9uPgogICAgICAgICAgICAg" +
                                "ICAgICAgICAgICA8Z3B4dHB4OnNwZWVkPjUuNjY8L2dweHRweDpzcGVlZD4KICAg" +
                                "ICAgICAgICAgICAgICAgICAgICAgPGdweHRweDpjb3Vyc2U+MTUxLjg4PC9ncHh0" +
                                "cHg6Y291cnNlPgogICAgICAgICAgICAgICAgICAgIDwvZ3B4dHB4OlRyYWNrUG9p" +
                                "bnRFeHRlbnNpb24+CiAgICAgICAgICAgICAgICAgICAgPHRyYWlsc2lvOlRyYWNr" +
                                "UG9pbnRFeHRlbnNpb24+CiAgICAgICAgICAgICAgICAgICAgICAgIDx0cmFpbHNp" +
                                "bzpoYWNjPjUuMDA8L3RyYWlsc2lvOmhhY2M+CiAgICAgICAgICAgICAgICAgICAg" +
                                "ICAgIDx0cmFpbHNpbzp2YWNjPjMuMDA8L3RyYWlsc2lvOnZhY2M+CiAgICAgICAg" +
                                "ICAgICAgICAgICAgICAgIDx0cmFpbHNpbzpzdGVwcz41NjEyPC90cmFpbHNpbzpz" +
                                "dGVwcz4KICAgICAgICAgICAgICAgICAgICA8L3RyYWlsc2lvOlRyYWNrUG9pbnRF" +
                                "eHRlbnNpb24+CiAgICAgICAgICAgICAgICA8L2V4dGVuc2lvbnM+CiAgICAgICAg" +
                                "ICAgIDwvdHJrcHQ+CiAgICAgICAgICAgIDx0cmtwdCBsb249IjUuNDg2MDU2IiBs" +
                                "YXQ9IjUxLjgwNTExNSI+CiAgICAgICAgICAgICAgICA8ZWxlPjkuMTIyMDAwPC9l" +
                                "bGU+CiAgICAgICAgICAgICAgICA8dGltZT4yMDE4LTA5LTA1VDE3OjMzOjI0Wjwv" +
                                "dGltZT4KICAgICAgICAgICAgICAgIDxleHRlbnNpb25zPgogICAgICAgICAgICAg" +
                                "ICAgICAgIDxncHh0cHg6VHJhY2tQb2ludEV4dGVuc2lvbj4KICAgICAgICAgICAg" +
                                "ICAgICAgICAgICAgPGdweHRweDpzcGVlZD40Ljc2PC9ncHh0cHg6c3BlZWQ+CiAg" +
                                "ICAgICAgICAgICAgICAgICAgICAgIDxncHh0cHg6Y291cnNlPjE1NC4zNDwvZ3B4" +
                                "dHB4OmNvdXJzZT4KICAgICAgICAgICAgICAgICAgICA8L2dweHRweDpUcmFja1Bv" +
                                "aW50RXh0ZW5zaW9uPgogICAgICAgICAgICAgICAgICAgIDx0cmFpbHNpbzpUcmFj" +
                                "a1BvaW50RXh0ZW5zaW9uPgogICAgICAgICAgICAgICAgICAgICAgICA8dHJhaWxz" +
                                "aW86aGFjYz41LjAwPC90cmFpbHNpbzpoYWNjPgogICAgICAgICAgICAgICAgICAg" +
                                "ICAgICA8dHJhaWxzaW86dmFjYz4zLjAwPC90cmFpbHNpbzp2YWNjPgogICAgICAg" +
                                "ICAgICAgICAgICAgICAgICA8dHJhaWxzaW86c3RlcHM+NTYyMTwvdHJhaWxzaW86" +
                                "c3RlcHM+CiAgICAgICAgICAgICAgICAgICAgPC90cmFpbHNpbzpUcmFja1BvaW50" +
                                "RXh0ZW5zaW9uPgogICAgICAgICAgICAgICAgPC9leHRlbnNpb25zPgogICAgICAg" +
                                "ICAgICA8L3Rya3B0PgogICAgICAgIDwvdHJrc2VnPgogICAgPC90cms+CjwvZ3B4" +
                                "Pg==";
	    
	    public static readonly byte[] Bytes = Base64Helper.TryParse(Base64GpxString);

	    
        public CreateAnGpx()
        {
            if (!File.Exists(FullFileGpxPath))
            {
                File.WriteAllBytes(FullFileGpxPath, Convert.FromBase64String(Base64GpxString));
            }
        }
    }
}
