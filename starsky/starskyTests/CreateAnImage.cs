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
            Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + Path.DirectorySeparatorChar + fileNameWithDate;
        private static readonly string fileNameWithDate = "123300_20120101.jpg";
                                            // HHmmss_yyyyMMdd > not very logical but used to test features

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

             if (!File.Exists(FullFilePath))
             {
                 File.WriteAllBytes(FullFilePath, Convert.FromBase64String(base64JpgString));
             }
             if (!File.Exists(FullFilePathWithDate))
             {
                 File.WriteAllBytes(FullFilePathWithDate, Convert.FromBase64String(base64JpgString1));
             }
         }
    }
}