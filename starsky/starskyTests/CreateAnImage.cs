using System;
using System.IO;
using System.Reflection;

namespace starskytests
{
    public class CreateAnImage
    {

        private static readonly string _fileName = "0000000000aaaaa__exifreadingtest00.jpg";
        // There is an unit test for using directory thumbnails that uses the first image;
        // starskytests.SyncServiceTest.SyncServiceFirstItemDirectoryTest

        public string FileName => _fileName;
        public readonly string DbPath = "/" + _fileName;
        public readonly string FullFilePath = 
            Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + Path.DirectorySeparatorChar + _fileName;
        public readonly string BasePath =
            Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + Path.DirectorySeparatorChar;

        public readonly string FullFilePathWithDate = 
            Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + Path.DirectorySeparatorChar + FileNameWithDate;
        private const string FileNameWithDate = "123300_20120101.jpg";
        // HHmmss_yyyyMMdd > not very logical but used to test features

        public readonly string FullFileGpxPath = 
            Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + Path.DirectorySeparatorChar + FileNameGpx;
        private const string FileNameGpx = "__test.gpx";


        public CreateAnImage()
        {
            var base64JpgString = "/9j/4AAQSkZJRgABAQABXgFeAAD/4QQgRXhpZgAATU0AKgAAAAgACwEOAAIAAAAg" +
                                   "AAAAkgEPAAIAAAAFAAAAsgEQAAIAAAAIAAAAuAESAAMAAAABAAEAAAEaAAUAAAAB" +
                                   "AAAAwAEbAAUAAAABAAAAyAEoAAMAAAABAAIAAAExAAIAAAAOAAAA0AEyAAIAAAAU" +
                                   "AAAA3odpAAQAAAABAAAA8oglAAQAAAABAAADNgAAAAAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgAFNPTlkAAFNMVC1BNTgAAAABXgAAAAEAAAFeAAAAAVNM" +
                                   "VC1BNTggdjEuMDAAMjAxODowNDoyMiAxNzo0MzowMAAAI4KaAAUAAAABAAACnIKd" +
                                   "AAUAAAABAAACpIgiAAMAAAABAAMAAIgnAAMAAAABAMgAAIgwAAMAAAABAAIAAIgy" +
                                   "AAQAAAABAAAAyJAAAAcAAAAEMDIzMJADAAIAAAAUAAACrJAEAAIAAAAUAAACwJEB" +
                                   "AAcAAAAEAQIDAJECAAUAAAABAAAC1JIDAAoAAAABAAAC3JIEAAoAAAABAAAC5JIF" +
                                   "AAUAAAABAAAC7JIHAAMAAAABAAUAAJIIAAMAAAABAAAAAJIJAAMAAAABABAAAJIK" +
                                   "AAUAAAABAAAC9KAAAAcAAAAEMDEwMKABAAMAAAABAAEAAKACAAQAAAABAAAAA6AD" +
                                   "AAQAAAABAAAAAqMAAAcAAAABAwAAAKMBAAcAAAABAQAAAKQBAAMAAAABAAAAAKQC" +
                                   "AAMAAAABAAAAAKQDAAMAAAABAAAAAKQEAAUAAAABAAAC/KQFAAMAAAABAJYAAKQG" +
                                   "AAMAAAABAAAAAKQIAAMAAAABAAAAAKQJAAMAAAABAAAAAKQKAAMAAAABAAAAAKQy" +
                                   "AAUAAAAEAAADBKQ0AAIAAAASAAADJAAAAAAAAAABAAAADwAAAA0AAAABMjAxODow" +
                                   "NDoyMiAxNjoxNDo1NAAyMDE4OjA0OjIyIDE2OjE0OjU0AAAAAAMAAAABAAAH5wAA" +
                                   "AUAAAAADAAAACgAAAJ8AAAAgAAAAZAAAAAEAAAABAAAAAQAAABgAAAABAAAAaQAA" +
                                   "AAEAAAAHAAAAAgAAAAkAAAACMjQtMTA1bW0gRjMuNS00LjUAAAoAAAABAAAABAIC" +
                                   "AAAAAQACAAAAAk4AAAAAAgAFAAAAAwAAA7QAAwACAAAAAkUAAAAABAAFAAAAAwAA" +
                                   "A8wABQABAAAAAQAAAAAABgAFAAAAAQAAA+QABwAFAAAAAwAAA+wAEgACAAAABwAA" +
                                   "BAQAHQACAAAACwAABAwAAAAAAAAANAAAAAEAAAASAAAAAQAAC4oAAABkAAAABgAA" +
                                   "AAEAAAALAAAAAQAADmAAAABkAAAYxQAAA+gAAAAOAAAAAQAAAA4AAAABAAAAIgAA" +
                                   "AAFXR1MtODQAADIwMTg6MDQ6MjIAAP/hIFdodHRwOi8vbnMuYWRvYmUuY29tL3hh" +
                                   "cC8xLjAvADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVT" +
                                   "ek5UY3prYzlkIj8+Cjx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8i" +
                                   "IHg6eG1wdGs9IlhNUCBDb3JlIDUuMS4yIj4KIDxyZGY6UkRGIHhtbG5zOnJkZj0i" +
                                   "aHR0cDovL3d3dy53My5vcmcvMTk5OS8wMi8yMi1yZGYtc3ludGF4LW5zIyI+CiAg" +
                                   "PHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIKICAgIHhtbG5zOmV4aWY9Imh0" +
                                   "dHA6Ly9ucy5hZG9iZS5jb20vZXhpZi8xLjAvIgogICAgeG1sbnM6ZXhpZkVYPSJo" +
                                   "dHRwOi8vY2lwYS5qcC9leGlmLzEuMC8iCiAgICB4bWxuczp4bXA9Imh0dHA6Ly9u" +
                                   "cy5hZG9iZS5jb20veGFwLzEuMC8iCiAgICB4bWxuczp0aWZmPSJodHRwOi8vbnMu" +
                                   "YWRvYmUuY29tL3RpZmYvMS4wLyIKICAgIHhtbG5zOmF1eD0iaHR0cDovL25zLmFk" +
                                   "b2JlLmNvbS9leGlmLzEuMC9hdXgvIgogICAgeG1sbnM6cGhvdG9zaG9wPSJodHRw" +
                                   "Oi8vbnMuYWRvYmUuY29tL3Bob3Rvc2hvcC8xLjAvIgogICAgeG1sbnM6ZGM9Imh0" +
                                   "dHA6Ly9wdXJsLm9yZy9kYy9lbGVtZW50cy8xLjEvIgogICAgeG1sbnM6SXB0YzR4" +
                                   "bXBFeHQ9Imh0dHA6Ly9pcHRjLm9yZy9zdGQvSXB0YzR4bXBFeHQvMjAwOC0wMi0y" +
                                   "OS8iCiAgICB4bWxuczpwaG90b21lY2hhbmljPSJodHRwOi8vbnMuY2FtZXJhYml0" +
                                   "cy5jb20vcGhvdG9tZWNoYW5pYy8xLjAvIgogICBleGlmOlNjZW5lVHlwZT0iMSIK" +
                                   "ICAgZXhpZjpHUFNBbHRpdHVkZVJlZj0iMCIKICAgZXhpZjpDb250cmFzdD0iMCIK" +
                                   "ICAgZXhpZjpDb21wcmVzc2VkQml0c1BlclBpeGVsPSIzLzEiCiAgIGV4aWY6R1BT" +
                                   "TGF0aXR1ZGU9IjUyLDE4LjQ5Mk4iCiAgIGV4aWY6R1BTVGltZVN0YW1wPSIyMDE4" +
                                   "LTA0LTIyVDE0OjE0OjM0KzAwMDAiCiAgIGV4aWY6RGlnaXRhbFpvb21SYXRpbz0i" +
                                   "MS8xIgogICBleGlmOlBpeGVsWURpbWVuc2lvbj0iMiIKICAgZXhpZjpDdXN0b21S" +
                                   "ZW5kZXJlZD0iMCIKICAgZXhpZjpNZXRlcmluZ01vZGU9IjUiCiAgIGV4aWY6UGl4" +
                                   "ZWxYRGltZW5zaW9uPSIzIgogICBleGlmOlNjZW5lQ2FwdHVyZVR5cGU9IjAiCiAg" +
                                   "IGV4aWY6Rm9jYWxMZW5JbjM1bW1GaWxtPSIxNTAiCiAgIGV4aWY6RXhwb3N1cmVN" +
                                   "b2RlPSIwIgogICBleGlmOkdQU0FsdGl0dWRlPSI2My8xMCIKICAgZXhpZjpTYXR1" +
                                   "cmF0aW9uPSIwIgogICBleGlmOkV4cG9zdXJlVGltZT0iMS8xNSIKICAgZXhpZjpT" +
                                   "aGFycG5lc3M9IjAiCiAgIGV4aWY6QnJpZ2h0bmVzc1ZhbHVlPSIyMDIzLzMyMCIK" +
                                   "ICAgZXhpZjpHUFNMb25naXR1ZGU9IjYsMTEuNjEzRSIKICAgZXhpZjpHUFNWZXJz" +
                                   "aW9uSUQ9IjIuMi4wLjAiCiAgIGV4aWY6RXhpZlZlcnNpb249IjAyMzAiCiAgIGV4" +
                                   "aWY6RmlsZVNvdXJjZT0iMyIKICAgZXhpZjpGbGFzaFBpeFZlcnNpb249IjAxMDAi" +
                                   "CiAgIGV4aWY6V2hpdGVCYWxhbmNlPSIwIgogICBleGlmOkNvbG9yU3BhY2U9IjEi" +
                                   "CiAgIGV4aWY6Rm9jYWxMZW5ndGg9IjEwMC8xIgogICBleGlmOkV4cG9zdXJlUHJv" +
                                   "Z3JhbT0iMyIKICAgZXhpZjpGTnVtYmVyPSIxMy8xIgogICBleGlmOk1heEFwZXJ0" +
                                   "dXJlVmFsdWU9IjE1OS8zMiIKICAgZXhpZjpHUFNNYXBEYXR1bT0iV0dTLTg0Igog" +
                                   "ICBleGlmOkxpZ2h0U291cmNlPSIwIgogICBleGlmOkV4cG9zdXJlQmlhc1ZhbHVl" +
                                   "PSIzLzEwIgogICBleGlmOkdQU0RhdGVUaW1lPSIyMDE4LTA0LTIyVDE0OjE0OjM0" +
                                   "WiIKICAgZXhpZkVYOlJlY29tbWVuZGVkRXhwb3N1cmVJbmRleD0iMjAwIgogICBl" +
                                   "eGlmRVg6UGhvdG9ncmFwaGljU2Vuc2l0aXZpdHk9IjIwMCIKICAgZXhpZkVYOkxl" +
                                   "bnNNb2RlbD0iMjQtMTA1bW0gRjMuNS00LjUiCiAgIGV4aWZFWDpTZW5zaXRpdml0" +
                                   "eVR5cGU9IjIiCiAgIHhtcDpDcmVhdG9yVG9vbD0iU0xULUE1OCB2MS4wMCIKICAg" +
                                   "eG1wOkNyZWF0ZURhdGU9IjIwMTgtMDQtMjJUMTY6MTQ6NTQiCiAgIHhtcDpNZXRh" +
                                   "ZGF0YURhdGU9IjIwMTgtMDQtMjJUMTc6NDM6MDArMDI6MDAiCiAgIHhtcDpNb2Rp" +
                                   "ZnlEYXRlPSIyMDE4LTA0LTIyVDE3OjQzOjAwKzAyOjAwIgogICB4bXA6TGFiZWw9" +
                                   "IiIKICAgeG1wOlJhdGluZz0iMCIKICAgdGlmZjpSZXNvbHV0aW9uVW5pdD0iMiIK" +
                                   "ICAgdGlmZjpPcmllbnRhdGlvbj0iMSIKICAgdGlmZjpYUmVzb2x1dGlvbj0iMzUw" +
                                   "LzEiCiAgIHRpZmY6WVJlc29sdXRpb249IjM1MC8xIgogICB0aWZmOk1vZGVsPSJT" +
                                   "TFQtQTU4IgogICB0aWZmOk1ha2U9IlNPTlkiCiAgIGF1eDpMZW5zPSJTaWdtYSAx" +
                                   "OC0yMDBtbSBGMy41LTYuMyBEQyIKICAgYXV4OkZsYXNoQ29tcGVuc2F0aW9uPSIw" +
                                   "LzEiCiAgIGF1eDpMZW5zSUQ9IjI0IgogICBwaG90b3Nob3A6Q2l0eT0iRGllcGVu" +
                                   "dmVlbiIKICAgcGhvdG9zaG9wOlN0YXRlPSJPdmVyaWpzc2VsIgogICBwaG90b3No" +
                                   "b3A6Q291bnRyeT0iTmVkZXJsYW5kIgogICBwaG90b3Nob3A6RGF0ZUNyZWF0ZWQ9" +
                                   "IjIwMTgtMDQtMjJUMTY6MTQ6NTQrMDE6MDAiCiAgIHBob3RvbWVjaGFuaWM6Q29s" +
                                   "b3JDbGFzcz0iMCIKICAgcGhvdG9tZWNoYW5pYzpUYWdnZWQ9IkZhbHNlIgogICBw" +
                                   "aG90b21lY2hhbmljOlByZWZzPSIwOjA6MDotMDAwMDEiCiAgIHBob3RvbWVjaGFu" +
                                   "aWM6UE1WZXJzaW9uPSJQTTUiPgogICA8ZXhpZjpGbGFzaAogICAgZXhpZjpGdW5j" +
                                   "dGlvbj0iRmFsc2UiCiAgICBleGlmOkZpcmVkPSJGYWxzZSIKICAgIGV4aWY6UmV0" +
                                   "dXJuPSIwIgogICAgZXhpZjpNb2RlPSIyIgogICAgZXhpZjpSZWRFeWVNb2RlPSJG" +
                                   "YWxzZSIvPgogICA8ZXhpZjpJU09TcGVlZFJhdGluZ3M+CiAgICA8cmRmOlNlcT4K" +
                                   "ICAgICA8cmRmOmxpPjIwMDwvcmRmOmxpPgogICAgPC9yZGY6U2VxPgogICA8L2V4" +
                                   "aWY6SVNPU3BlZWRSYXRpbmdzPgogICA8ZXhpZjpDb21wb25lbnRzQ29uZmlndXJh" +
                                   "dGlvbj4KICAgIDxyZGY6U2VxPgogICAgIDxyZGY6bGk+MTwvcmRmOmxpPgogICAg" +
                                   "IDxyZGY6bGk+MjwvcmRmOmxpPgogICAgIDxyZGY6bGk+MzwvcmRmOmxpPgogICAg" +
                                   "IDxyZGY6bGk+MDwvcmRmOmxpPgogICAgPC9yZGY6U2VxPgogICA8L2V4aWY6Q29t" +
                                   "cG9uZW50c0NvbmZpZ3VyYXRpb24+CiAgIDxleGlmRVg6TGVuc1NwZWNpZmljYXRp" +
                                   "b24+CiAgICA8cmRmOlNlcT4KICAgICA8cmRmOmxpPjI0LzE8L3JkZjpsaT4KICAg" +
                                   "ICA8cmRmOmxpPjEwNS8xPC9yZGY6bGk+CiAgICAgPHJkZjpsaT43LzI8L3JkZjps" +
                                   "aT4KICAgICA8cmRmOmxpPjkvMjwvcmRmOmxpPgogICAgPC9yZGY6U2VxPgogICA8" +
                                   "L2V4aWZFWDpMZW5zU3BlY2lmaWNhdGlvbj4KICAgPGRjOnN1YmplY3Q+CiAgICA8" +
                                   "cmRmOkJhZz4KICAgICA8cmRmOmxpPnRlc3Q8L3JkZjpsaT4KICAgICA8cmRmOmxp" +
                                   "PnNpb248L3JkZjpsaT4KICAgIDwvcmRmOkJhZz4KICAgPC9kYzpzdWJqZWN0Pgog" +
                                   "ICA8ZGM6ZGVzY3JpcHRpb24+CiAgICA8cmRmOkFsdD4KICAgICA8cmRmOmxpIHht" +
                                   "bDpsYW5nPSJ4LWRlZmF1bHQiPmNhcHRpb248L3JkZjpsaT4KICAgIDwvcmRmOkFs" +
                                   "dD4KICAgPC9kYzpkZXNjcmlwdGlvbj4KICAgPGRjOnRpdGxlPgogICAgPHJkZjpB" +
                                   "bHQ+CiAgICAgPHJkZjpsaSB4bWw6bGFuZz0ieC1kZWZhdWx0Ij50aXRsZTwvcmRm" +
                                   "OmxpPgogICAgPC9yZGY6QWx0PgogICA8L2RjOnRpdGxlPgogICA8SXB0YzR4bXBF" +
                                   "eHQ6TG9jYXRpb25DcmVhdGVkPgogICAgPHJkZjpCYWc+CiAgICAgPHJkZjpsaQog" +
                                   "ICAgICBJcHRjNHhtcEV4dDpTdWJsb2NhdGlvbj0iIgogICAgICBJcHRjNHhtcEV4" +
                                   "dDpDaXR5PSJEaWVwZW52ZWVuIgogICAgICBJcHRjNHhtcEV4dDpQcm92aW5jZVN0" +
                                   "YXRlPSJPdmVyaWpzc2VsIgogICAgICBJcHRjNHhtcEV4dDpDb3VudHJ5TmFtZT0i" +
                                   "TmVkZXJsYW5kIgogICAgICBJcHRjNHhtcEV4dDpDb3VudHJ5Q29kZT0iIgogICAg" +
                                   "ICBJcHRjNHhtcEV4dDpXb3JsZFJlZ2lvbj0iIi8+CiAgICA8L3JkZjpCYWc+CiAg" +
                                   "IDwvSXB0YzR4bXBFeHQ6TG9jYXRpb25DcmVhdGVkPgogICA8SXB0YzR4bXBFeHQ6" +
                                   "TG9jYXRpb25TaG93bj4KICAgIDxyZGY6QmFnPgogICAgIDxyZGY6bGkKICAgICAg" +
                                   "SXB0YzR4bXBFeHQ6U3VibG9jYXRpb249IiIKICAgICAgSXB0YzR4bXBFeHQ6Q2l0" +
                                   "eT0iRGllcGVudmVlbiIKICAgICAgSXB0YzR4bXBFeHQ6UHJvdmluY2VTdGF0ZT0i" +
                                   "T3Zlcmlqc3NlbCIKICAgICAgSXB0YzR4bXBFeHQ6Q291bnRyeU5hbWU9Ik5lZGVy" +
                                   "bGFuZCIKICAgICAgSXB0YzR4bXBFeHQ6Q291bnRyeUNvZGU9IiIKICAgICAgSXB0" +
                                   "YzR4bXBFeHQ6V29ybGRSZWdpb249IiIvPgogICAgPC9yZGY6QmFnPgogICA8L0lw" +
                                   "dGM0eG1wRXh0OkxvY2F0aW9uU2hvd24+CiAgPC9yZGY6RGVzY3JpcHRpb24+CiA8" +
                                   "L3JkZjpSREY+CjwveDp4bXBtZXRhPgogICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAK" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "IAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAog" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAg" +
                                   "ICAgICAgICAgICAgICAgICAgICAgICAgICAgCjw/eHBhY2tldCBlbmQ9InciPz7/" +
                                   "7QDiUGhvdG9zaG9wIDMuMAA4QklNBAQAAAAAAKkcAVoAAxslRxwCAAACAAMcAjcA" +
                                   "CDIwMTgwNDIyHAI8AAsxNjE0NTQrMDEwMBwCWgAKRGllcGVudmVlbhwCXwAKT3Zl" +
                                   "cmlqc3NlbBwCZQAJTmVkZXJsYW5kHAIFAAV0aXRsZRwCGQAEdGVzdBwCGQAEc2lv" +
                                   "bhwCeAAHY2FwdGlvbhwC3QAMMDowOjA6LTAwMDAxHAI+AAgyMDE4MDQyMhwCPwAG" +
                                   "MTYxNDU0ADhCSU0EJQAAAAAAEOXrWd9hKhRBbyqe22Ymbij/2wCEAAEBAQEBAQIC" +
                                   "AgICAgICAgQDAgIDBAUEAwMDBAUHBQQDAwQFBwcGBQQFBgcIBgUFBggIBwcHCAkI" +
                                   "CAkKCgoMDA4BAgICAgICAwICAwYDAgMGDAYEBAYMDwwHBQcMDw8PDQkJDQ8PDw8P" +
                                   "Dg8PDw8PDw8PDw8PDw8PDw8PDw8PDw8PD//CABEIAAIAAwMBIQACEQEDEQH/xAAU" +
                                   "AAEAAAAAAAAAAAAAAAAAAAAJ/9oACAEBAAAAAEV//8QAFAEBAAAAAAAAAAAAAAAA" +
                                   "AAAABf/aAAgBAhAAAAAH/8QAFAEBAAAAAAAAAAAAAAAAAAAAA//aAAgBAxAAAAAf" +
                                   "/8QAIBAAAQMCBwAAAAAAAAAAAAAAAQIGIQQRAAMFEiMxUf/aAAgBAQABPwBhNVrp" +
                                   "aFBbTaAXydx4USpRJJMdkyT7j//EABsRAAIBBQAAAAAAAAAAAAAAAAECBQADBBFy" +
                                   "/9oACAECAQE/AFjo8qCcW3vkV//EABkRAQACAwAAAAAAAAAAAAAAAAEAAhESIf/a" +
                                   "AAgBAwEBPwDaxwXE/9k=";

             var base64JpgString1 =
                 "/9j/4AAQSkZJRgABAQAAAQABAAD/2wDFAAEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEAAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQABAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEB/8EAEQgAAgADAwARAAERAAIRAP/EACcAAQEAAAAAAAAAAAAAAAAAAAAKEAEAAAAAAAAAAAAAAAAAAAAA/9oADAMAAAEAAgAAPwC/gH//2Q==";

            var base64gpxFile = "CjxncHggeG1sbnM9Imh0dHA6Ly93d3cudG9wb2dyYWZpeC5jb20vR1BYLzEvMSIg" +
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
            
            if (!File.Exists(FullFilePath))
            {
                File.WriteAllBytes(FullFilePath, Convert.FromBase64String(base64JpgString));
            }
            if (!File.Exists(FullFilePathWithDate))
            {
                File.WriteAllBytes(FullFilePathWithDate, Convert.FromBase64String(base64JpgString1));
            }
            
            if (!File.Exists(FullFileGpxPath))
            {
                File.WriteAllBytes(FullFileGpxPath, Convert.FromBase64String(base64gpxFile));
            }
         }
    }
}