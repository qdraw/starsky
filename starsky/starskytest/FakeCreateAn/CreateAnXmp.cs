using System.Text;

namespace starskytest.FakeCreateAn
{
	public static class CreateAnXmp
	{
		private static readonly string XmpString =
			"<x:xmpmeta xmlns:x=\"adobe:ns:meta/\" x:xmptk=\"Image::ExifTool 11.16\"><rdf:RDF xmlns:" +
			"rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\"><rdf:Description xmlns:exif=\"http:" +
			"//ns.adobe.com/exif/1.0/\" rdf:about=\"\"><exif:DateTimeOriginal>2020-03-14T14:00:51</e" +
			"xif:DateTimeOriginal><exif:ExposureTime>1/1600</exif:ExposureTime><exif:FNumber>11/5</e" +
			"xif:FNumber><exif:FocalLength>4404019/1048576</exif:FocalLength><exif:GPSAltitude>1/1</e" +
			"xif:GPSAltitude><exif:GPSAltitudeRef>0</exif:GPSAltitudeRef><exif:GPSLatitude>52,15.73N<" +
			"/exif:GPSLatitude><exif:GPSLongitude>6,2.37E</exif:GPSLongitude><exif:ISOSpeedRatings><r" +
			"df:Seq><rdf:li>25</rdf:li><rdf:li>25</rdf:li></rdf:Seq></exif:ISOSpeedRatings></rdf:Desc" +
			"ription><rdf:Description xmlns:photomechanic=\"http://ns.camerabits.com/photomechanic/1." +
			"0/\" rdf:about=\"\"><photomechanic:ColorClass>7</photomechanic:ColorClass><photomechanic" +
			":Prefs>0:7:0:0</photomechanic:Prefs></rdf:Description><rdf:Description xmlns:photoshop=\"" +
			"http://ns.adobe.com/photoshop/1.0/\" rdf:about=\"\"><photoshop:DateCreated>2020-03-14T14" +
			":00:51</photoshop:DateCreated></rdf:Description><rdf:Description xmlns:tiff=\"http://ns." +
			"adobe.com/tiff/1.0/\" rdf:about=\"\"><tiff:Make>Apple</tiff:Make><tiff:Model>iPhone SE<" +
			"/tiff:Model><tiff:Orientation>3</tiff:Orientation><tiff:Software>Qdraw 1.0</tiff:Softwa" +
			"re></rdf:Description><rdf:Description xmlns:xmp=\"http://ns.adobe.com/xap/1.0/\" rdf:ab" +
			"out=\"\"><xmp:CreateDate>2020-03-14T14:00:51</xmp:CreateDate><xmp:CreatorTool>Qdraw 1.0" +
			"</xmp:CreatorTool><xmp:Label>Extras</xmp:Label><xmp:ModifyDate>2020-03-14T14:00:51</xmp" +
			":ModifyDate></rdf:Description><rdf:Description xmlns:stEvt=\"http://ns.adobe.com/xap/1." +
			"0/sType/ResourceEvent#\" xmlns:xmpMM=\"http://ns.adobe.com/xap/1.0/mm/\" rdf:about=\"\"" +
			"><xmpMM:History><rdf:Seq><rdf:li rdf:parseType=\"Resource\"><stEvt:softwareAgent>Qdraw " +
			"1.0</stEvt:softwareAgent></rdf:li></rdf:Seq></xmpMM:History></rdf:Description></rdf:RDF>" +
			"</x:xmpmeta>";
		
		 public static readonly byte[] Bytes =  Encoding.ASCII.GetBytes(XmpString);
	}
}
