using System.Collections.Generic;
using System.Linq;

namespace starsky.foundation.readmeta.Helpers
{
	/// <summary>
	/// @see: https://exiftool.org/TagNames/Sony.html
	/// </summary>
	public class SonyLensIdConverter
	{

		private static Dictionary<string, string> SonyIdDict =>
			new Dictionary<string, string>
			{
				{"0","Minolta AF 28-85mm F3.5-4.5 New"}, // # New added (ref 13/18)
				{"1","Minolta AF 80-200mm F2.8 HS-APO G"}, // # white
				{"2","Minolta AF 28-70mm F2.8 G"},
				{"3","Minolta AF 28-80mm F4-5.6"},
				{"4","Minolta AF 85mm F1.4G"}, // #exiv2 0.23
				{"5","Minolta AF 35-70mm F3.5-4.5 [II]"}, // # (original and II, ref 13)
				{"6","Minolta AF 24-85mm F3.5-4.5 [New]"}, // # (original and New, ref 13)
				{"7","Minolta AF 100-300mm F4.5-5.6 APO [New] or 100-400mm or Sigma Lens"},
				{"8","Minolta AF 70-210mm F4.5-5.6 [II]"}, // # (original and II, ref 13)
				{"9","Minolta AF 50mm F3.5 Macro"},
				{"10","Minolta AF 28-105mm F3.5-4.5 [New]"}, // # (original and New, ref 13)
				{"11","Minolta AF 300mm F4 HS-APO G"},
				{"12","Minolta AF 100mm F2.8 Soft Focus"},
				{"13","Minolta AF 75-300mm F4.5-5.6 (New or II)"}, // # (II and New, ref 13)
				{"14","Minolta AF 100-400mm F4.5-6.7 APO"},
				{"15","Minolta AF 400mm F4.5 HS-APO G"},
				{"16","Minolta AF 17-35mm F3.5 G"},
				{"17","Minolta AF 20-35mm F3.5-4.5"},
				{"18","Minolta AF 28-80mm F3.5-5.6 II"},
				{"19","Minolta AF 35mm F1.4 G"}, // # G added (ref 18), but not New as per ref 13
				{"20","Minolta/Sony 135mm F2.8 [T4.5] STF"},
				{"22","Minolta AF 35-80mm F4-5.6 II"}, // # II added (ref 13)
				{"23","Minolta AF 200mm F4 Macro APO G"},
				{"24","Minolta/Sony AF 24-105mm F3.5-4.5 (D) or Sigma or Tamron Lens"},
				{"25","Minolta AF 100-300mm F4.5-5.6 APO (D) or Sigma Lens"},
				{"27","Minolta AF 85mm F1.4 G (D)"}, // # added (D) (ref 13)
				{"28","Minolta/Sony AF 100mm F2.8 Macro (D) or Tamron Lens"},
				{"28.1","Tamron SP AF 90mm F2.8 Di Macro"}, // #JD (Model 272E)
				{"28.2","Tamron SP AF 180mm F3.5 Di LD [IF] Macro"}, // #27 (Model B01) ("SP" moved - ref JR)
				{"29","Minolta/Sony AF 75-300mm F4.5-5.6 (D)"}, // # Sony added (ref 13)
				{"30","Minolta AF 28-80mm F3.5-5.6 (D) or Sigma Lens"},
				{"30.1","Sigma AF 10-20mm F4-5.6 EX DC"}, // #JD
				{"30.2","Sigma AF 12-24mm F4.5-5.6 EX DG"},
				{"30.3","Sigma 28-70mm EX DG F2.8"}, // #16
				{"30.4","Sigma 55-200mm F4-5.6 DC"}, // #JD
				{"31","Minolta/Sony AF 50mm F2.8 Macro (D) or F3.5"},
				{"31.1","Minolta/Sony AF 50mm F3.5 Macro"},
				{"32","Minolta/Sony AF 300mm F2.8 G or 1.5x Teleconverter"}, // #13/18
				{"33","Minolta/Sony AF 70-200mm F2.8 G"},
				{"35","Minolta AF 85mm F1.4 G (D) Limited"},
				{"36","Minolta AF 28-100mm F3.5-5.6 (D)"},
				{"38","Minolta AF 17-35mm F2.8-4 (D)"}, // # (Konica Minolta, ref 13)
				{"39","Minolta AF 28-75mm F2.8 (D)"}, // # (Konica Minolta, ref 13)
				{"40","Minolta/Sony AF DT 18-70mm F3.5-5.6 (D)"}, // # (Konica Minolta, ref 13)
				{"41","Minolta/Sony AF DT 11-18mm F4.5-5.6 (D) or Tamron Lens"}, // # (Konica Minolta, ref 13)
				{"41.1","Tamron SP AF 11-18mm F4.5-5.6 Di II LD Aspherical IF"}, // #JD (Model A13)
				{"42","Minolta/Sony AF DT 18-200mm F3.5-6.3 (D)"}, // # Sony added (ref 13) (Konica Minolta, ref 13)
				{"43","Sony 35mm F1.4 G (SAL35F14G)"}, // # changed from Minolta to Sony (ref 13/18/JR) (but ref 11 shows both!)
				{"44","Sony 50mm F1.4 (SAL50F14)"}, // # changed from Minolta to Sony (ref 13/18/JR)
				{"45","Carl Zeiss Planar T* 85mm F1.4 ZA (SAL85F14Z)"}, // #JR
				{"46","Carl Zeiss Vario-Sonnar T* DT 16-80mm F3.5-4.5 ZA (SAL1680Z)"}, // #JR
				{"47","Carl Zeiss Sonnar T* 135mm F1.8 ZA (SAL135F18Z)"}, // #JR
				{"48","Carl Zeiss Vario-Sonnar T* 24-70mm F2.8 ZA SSM (SAL2470Z) or Other Lens"}, // #11/JR
				{"48.1","Carl Zeiss Vario-Sonnar T* 24-70mm F2.8 ZA SSM II (SAL2470Z2)"}, // #JR
				{"48.2","Tamron SP 24-70mm F2.8 Di USD"}, // #IB (A007) (also with id 204)
				{"49","Sony DT 55-200mm F4-5.6 (SAL55200)"}, // #JD/JR
				{"50","Sony DT 18-250mm F3.5-6.3 (SAL18250)"}, // #11/JR
				{"51","Sony DT 16-105mm F3.5-5.6 (SAL16105)"}, // #11/JR
				{"52","Sony 70-300mm F4.5-5.6 G SSM (SAL70300G) or G SSM II or Tamron Lens"}, // #JD
				{"52.1","Sony 70-300mm F4.5-5.6 G SSM II (SAL70300G2)"}, // #JR
				{"52.2","Tamron SP 70-300mm F4-5.6 Di USD"}, // #JR,NJ (Model A005)
				{"53","Sony 70-400mm F4-5.6 G SSM (SAL70400G)"}, // #17(/w correction by Stephen Bishop)/JR
				{"54","Carl Zeiss Vario-Sonnar T* 16-35mm F2.8 ZA SSM (SAL1635Z) or ZA SSM II"}, // #17/JR
				{"54.1","Carl Zeiss Vario-Sonnar T* 16-35mm F2.8 ZA SSM II (SAL1635Z2)"}, // #JR
				{"55","Sony DT 18-55mm F3.5-5.6 SAM (SAL1855) or SAM II"}, // #PH
				{"55.1","Sony DT 18-55mm F3.5-5.6 SAM II (SAL18552)"}, // #JR
				{"56","Sony DT 55-200mm F4-5.6 SAM (SAL55200-2)"}, // #22/JR
				{"57","Sony DT 50mm F1.8 SAM (SAL50F18) or Tamron Lens or Commlite CM-EF-NEX adapter"}, // #22/JR
				{"57.1","Tamron SP AF 60mm F2 Di II LD [IF] Macro 1:1"}, // # (Model G005) (ref https://exiftool.org/forum/index.php/topic,3858.0.html)
				{"57.2","Tamron 18-270mm F3.5-6.3 Di II PZD"}, // #27 (Model B008)
				//  {"# (note: the Commlite CM-EF-NEX adapter also appears to give LensType 57, ref JR)
				{"58","Sony DT 30mm F2.8 Macro SAM (SAL30M28)"}, // #22/JR
				{"59","Sony 28-75mm F2.8 SAM (SAL2875)"}, // #21/JR
				{"60","Carl Zeiss Distagon T* 24mm F2 ZA SSM (SAL24F20Z)"}, // #17/JR
				{"61","Sony 85mm F2.8 SAM (SAL85F28)"}, // #17/JR
				{"62","Sony DT 35mm F1.8 SAM (SAL35F18)"}, // #PH/JR
				{"63","Sony DT 16-50mm F2.8 SSM (SAL1650)"}, // #17/JR
				{"64","Sony 500mm F4 G SSM (SAL500F40G)"}, // #29
				{"65","Sony DT 18-135mm F3.5-5.6 SAM (SAL18135)"}, // #JR
				{"66","Sony 300mm F2.8 G SSM II (SAL300F28G2)"}, // #29
				{"67","Sony 70-200mm F2.8 G SSM II (SAL70200G2)"}, // #JR
				{"68","Sony DT 55-300mm F4.5-5.6 SAM (SAL55300)"}, // #29
				{"69","Sony 70-400mm F4-5.6 G SSM II (SAL70400G2)"}, // #JR
				{"70","Carl Zeiss Planar T* 50mm F1.4 ZA SSM (SAL50F14Z)"}, // #JR
				{"128","Tamron or Sigma Lens"}, //  (128)
				{"128.1","Tamron AF 18-200mm F3.5-6.3 XR Di II LD Aspherical [IF] Macro"}, // #JR (Model A14)
				{"128.2","Tamron AF 28-300mm F3.5-6.3 XR Di LD Aspherical [IF] Macro"}, // #JR (Model A061)
				// was 128.2","Tamron 28-300mm F3.5-6.3"},
				//  {"# (removed -- probably never existed, ref IB) 'Tamron 80-300mm F3.5-6.3"},
				{"128.3","Tamron AF 28-200mm F3.8-5.6 XR Di Aspherical [IF] Macro"}, // #JD (Model A031)
				// # also Tamron AF 28-200mm F3.8-5.6 Aspherical"}, // #IB (Model 71D)
				// # and 'Tamron AF 28-200mm F3.8-5.6 LD Aspherical [IF] Super"}, // #IB (Model 171D)
				{"128.4","Tamron SP AF 17-35mm F2.8-4 Di LD Aspherical IF"}, // #JD (Model A05)
				{"128.5","Sigma AF 50-150mm F2.8 EX DC APO HSM II"}, // #JD
				{"128.6","Sigma 10-20mm F3.5 EX DC HSM"}, // #11 (Model 202-205)
				{"128.7","Sigma 70-200mm F2.8 II EX DG APO MACRO HSM"}, // #24
				{"128.8","Sigma 10mm F2.8 EX DC HSM Fisheye"}, // #Florian Knorn
				//(yes, '128.10'.  My condolences to typed languages that use this database - PH)
				{"128.9","Sigma 50mm F1.4 EX DG HSM"}, // #Florian Knorn (Model A014, ref IB)
				{"128.10'","Sigma 85mm F1.4 EX DG HSM"}, // #27
				{"128.11'","Sigma 24-70mm F2.8 IF EX DG HSM"}, // #27
				{"128.12'","Sigma 18-250mm F3.5-6.3 DC OS HSM"}, // #27
				{"128.13'","Sigma 17-50mm F2.8 EX DC HSM"}, // #Exiv2
				{"128.14'","Sigma 17-70mm F2.8-4 DC Macro HSM"}, // # (no OS for Sony mount, ref JR) (also C013 Model, ref IB)
				{"128.15'","Sigma 150mm F2.8 EX DG OS HSM APO Macro"}, // #Marcus Holland-Moritz
				{"128.16'","Sigma 150-500mm F5-6.3 APO DG OS HSM"}, // #IB
				{"128.17'","Tamron AF 28-105mm F4-5.6 [IF]"}, // #IB (Model 179D)
				{"128.18'","Sigma 35mm F1.4 DG HSM"}, // #JR
				{"128.19'","Sigma 18-35mm F1.8 DC HSM"}, // #JR (Model A013, ref IB)
				{"128.20'","Sigma 50-500mm F4.5-6.3 APO DG OS HSM"}, // #JR
				{"128.21'","Sigma 24-105mm F4 DG HSM | A"}, // #JR (013)
				{"128.22'","Sigma 30mm F1.4"}, // #IB
				{"128.23'","Sigma 35mm F1.4 DG HSM | A"}, // #IB/JR (012)
				{"128.24'","Sigma 105mm F2.8 EX DG OS HSM Macro"}, // #IB
				{"128.25'","Sigma 180mm F2.8 EX DG OS HSM APO Macro"}, // #IB
				{"128.26'","Sigma 18-300mm F3.5-6.3 DC Macro HSM | C"}, // #IB/JR (014)
				{"128.27'","Sigma 18-50mm F2.8-4.5 DC HSM"}, // #IB
				{"129","Tamron Lens"},
				{"129.1","Tamron 200-400mm F5.6 LD"}, // #12 (LD ref 23)
				{"129.2","Tamron 70-300mm F4-5.6 LD"}, // #12
				{"131","Tamron 20-40mm F2.7-3.5 SP Aspherical IF"}, // #23 (Model 266D)
				{"135","Vivitar 28-210mm F3.5-5.6"}, // #16
				{"136","Tokina EMZ M100 AF 100mm F3.5"}, // #JD
				{"137","Cosina 70-210mm F2.8-4 AF"}, // #11
				{"138","Soligor 19-35mm F3.5-4.5"}, // #11
				{"139","Tokina AF 28-300mm F4-6.3"}, // #IB
				//(the following Cosina 70-300mm lens was also marketed as a Phoenix, Vivitar Series 1, and
				//some sort of 3rd-party marketing as a Voightlander 70-300mm F4.5-5.6 SKOPAR AF, ref IB)
				{"142","Cosina AF 70-300mm F4.5-5.6 MC"}, // #IB (was 'Voigtlander 70-300mm F4.5-5.6"}, // #JD)
				{"146","Voigtlander Macro APO-Lanthar 125mm F2.5 SL"}, // #JD
				{"194","Tamron SP AF 17-50mm F2.8 XR Di II LD Aspherical [IF]"}, // #23 (Model A16)
				{"202","Tamron SP AF 70-200mm F2.8 Di LD [IF] Macro"}, // #JR (Model A001) (see also 255.7)
				{"203","Tamron SP 70-200mm F2.8 Di USD"}, // #JR (Model A009)
				{"204","Tamron SP 24-70mm F2.8 Di USD"}, // #JR (Model A007) (also with id 48)
				{"212","Tamron 28-300mm F3.5-6.3 Di PZD"}, // #JR (Model A010)
				{"213","Tamron 16-300mm F3.5-6.3 Di II PZD Macro"}, // #JR (Model B016)
				{"214","Tamron SP 150-600mm F5-6.3 Di USD"}, // #JR (Model A011)
				{"215","Tamron SP 15-30mm F2.8 Di USD"}, // #JR (Model A012)
				{"216","Tamron SP 45mm F1.8 Di USD"}, // #forum8320 (F013)
				{"217","Tamron SP 35mm F1.8 Di USD"}, // #forum8320 (F012)
				{"218","Tamron SP 90mm F2.8 Di Macro 1:1 USD (F017)"}, // #JR (Model F017)
				{"220","Tamron SP 150-600mm F5-6.3 Di USD G2"}, // #forum8846 (Model A022)
				{"224","Tamron SP 90mm F2.8 Di Macro 1:1 USD (F004)"}, // #JR (Model F004)
				{"255","Tamron Lens (255)"},
				{"255.1","Tamron SP AF 17-50mm F2.8 XR Di II LD Aspherical"}, // # (Model A16)
				{"255.2","Tamron AF 18-250mm F3.5-6.3 XR Di II LD"}, // #JD (Model A18?)
				// #? 225.2","Tamron AF 18-250mm F3.5-6.3 Di II LD Aspherical [IF] Macro"}, // #JR (Model A18)
				{"255.3","Tamron AF 55-200mm F4-5.6 Di II LD Macro"}, // # (Model A15) (added "LD Macro", ref 23)
				{"255.4","Tamron AF 70-300mm F4-5.6 Di LD Macro 1:2"}, // # (Model A17)
				{"255.5","Tamron SP AF 200-500mm F5.0-6.3 Di LD IF"}, // # (Model A08)
				{"255.6","Tamron SP AF 10-24mm F3.5-4.5 Di II LD Aspherical IF"}, // #22 (Model B001)
				{"255.7","Tamron SP AF 70-200mm F2.8 Di LD IF Macro"}, // #22 (Model A001)
				{"255.8","Tamron SP AF 28-75mm F2.8 XR Di LD Aspherical IF"}, // #24 (Model A09)
				{"255.9","Tamron AF 90-300mm F4.5-5.6 Telemacro"}, // #Fredrik Agert
				{"18688","Sigma MC-11 SA-E Mount Converter with not-supported Sigma lens"},
				//The MC-11 SA-E Mount Converter uses this 18688 offset for not-supported SIGMA mount lenses.
				//The MC-11 EF-E Mount Converter uses the 61184 offset for not-supported CANON mount lenses, as also used by Metabones.
				//Both MC-11 SA-E and EF-E Mount Converters use the 504xx LensType2 values for supported SA-mount or EF-mount Sigma lenses.
				{"25501","Minolta AF 50mm F1.7"}, // #7
				{"25511","Minolta AF 35-70mm F4 or Other Lens"},
				{"25511.1","Sigma UC AF 28-70mm F3.5-4.5"}, // #12/16(HighSpeed-AF)
				{"25511.2","Sigma AF 28-70mm F2.8"}, // #JD
				{"25511.3","Sigma M-AF 70-200mm F2.8 EX Aspherical"}, // #12
				{"25511.4","Quantaray M-AF 35-80mm F4-5.6"}, // #JD
				{"25511.5","Tokina 28-70mm F2.8-4.5 AF"}, // #IB
				{"25521","Minolta AF 28-85mm F3.5-4.5 or Other Lens"}, // # not New (ref 18)
				{"25521.1","Tokina 19-35mm F3.5-4.5"}, // #3
				{"25521.2","Tokina 28-70mm F2.8 AT-X"}, // #7
				{"25521.3","Tokina 80-400mm F4.5-5.6 AT-X AF II 840"}, // #JD
				{"25521.4","Tokina AF PRO 28-80mm F2.8 AT-X 280"}, // #JD
				{"25521.5","Tokina AT-X PRO [II] AF 28-70mm F2.6-2.8 270"}, // #24 (original + II versions)
				{"25521.6","Tamron AF 19-35mm F3.5-4.5"}, // #JD (Model A10)
				{"25521.7","Angenieux AF 28-70mm F2.6"}, // #JD
				{"25521.8","Tokina AT-X 17 AF 17mm F3.5"}, // #27
				{"25521.9","Tokina 20-35mm F3.5-4.5 II AF"}, // #IB
				{"25531","Minolta AF 28-135mm F4-4.5 or Other Lens"},
				{"25531.1","Sigma ZOOM-alpha 35-135mm F3.5-4.5"}, // #16
				{"25531.2","Sigma 28-105mm F2.8-4 Aspherical"}, // #JD
				{"25531.3","Sigma 28-105mm F4-5.6 UC"}, // #JR
				{"25531.4","Tokina AT-X 242 AF 24-200mm F3.5-5.6"}, // #IB
				{"25541","Minolta AF 35-105mm F3.5-4.5"}, // #13
				{"25551","Minolta AF 70-210mm F4 Macro or Sigma Lens"},
				{"25551.1","Sigma 70-210mm F4-5.6 APO"}, // #7
				{"25551.2","Sigma M-AF 70-200mm F2.8 EX APO"}, // #6
				{"25551.3","Sigma 75-200mm F2.8-3.5"}, // #22
				{"25561","Minolta AF 135mm F2.8"},
				{"25571","Minolta/Sony AF 28mm F2.8"}, // # Sony added (ref 18)
				// 25571","Sony 28mm F2.8 (SAL28F28)"}, // (ref 18/JR)
				{"25581","Minolta AF 24-50mm F4"},
				{"25601","Minolta AF 100-200mm F4.5"},
				{"25611","Minolta AF 75-300mm F4.5-5.6 or Sigma Lens"}, // #13
				{"25611.1","Sigma 70-300mm F4-5.6 DL Macro"}, // #12 (also DG version ref 27, and APO version ref JR)
				{"25611.2","Sigma 300mm F4 APO Macro"}, // #3/7
				{"25611.3","Sigma AF 500mm F4.5 APO"}, // #JD
				{"25611.4","Sigma AF 170-500mm F5-6.3 APO Aspherical"}, // #JD
				{"25611.5","Tokina AT-X AF 300mm F4"}, // #JD
				{"25611.6","Tokina AT-X AF 400mm F5.6 SD"}, // #22
				{"25611.7","Tokina AF 730 II 75-300mm F4.5-5.6"}, // #JD
				{"25611.8","Sigma 800mm F5.6 APO"}, // #https://exiftool.org/forum/index.php/topic,3472.0.html
				{"25611.9","Sigma AF 400mm F5.6 APO Macro"}, // #27
				{"25611.10'","Sigma 1000mm F8 APO"}, // #JR
				{"25621","Minolta AF 50mm F1.4 [New]"}, // # original and New, not Sony (ref 13/18)
				{"25631","Minolta AF 300mm F2.8 APO or Sigma Lens"}, // # changed G to APO (ref 13)
				{"25631.1","Sigma AF 50-500mm F4-6.3 EX DG APO"}, // #JD
				{"25631.2","Sigma AF 170-500mm F5-6.3 APO Aspherical"}, // #JD (also DG version, ref 27)
				{"25631.3","Sigma AF 500mm F4.5 EX DG APO"}, // #JD
				{"25631.4","Sigma 400mm F5.6 APO"}, // #22
				{"25641","Minolta AF 50mm F2.8 Macro or Sigma Lens"},
				{"25641.1","Sigma 50mm F2.8 EX Macro"}, // #11
				{"25651","Minolta AF 600mm F4 APO"}, // # ("APO" added - ref JR)
				{"25661","Minolta AF 24mm F2.8 or Sigma Lens"},
				{"25721","Minolta/Sony AF 500mm F8 Reflex"},
				{"25781","Minolta/Sony AF 16mm F2.8 Fisheye or Sigma Lens"}, // # Sony added (ref 13/18)
				{"25791","Minolta/Sony AF 20mm F2.8 or Tokina Lens"}, // # Sony added (ref 11)
				// 25791","Sony 20mm F2.8 (SAL20F28)"}, // (ref JR)
				{"25791.1","Tokina AT-X Pro DX 11-16mm F2.8"}, // #https://exiftool.org/forum/index.php/topic,3593.0.html
				{"25811","Minolta AF 100mm F2.8 Macro [New] or Sigma or Tamron Lens"}, // # not Sony (ref 13/18)
				{"25851","Beroflex 35-135mm F3.5-4.5"}, // #16
				{"25858","Minolta AF 35-105mm F3.5-4.5 New or Tamron Lens"},
				{"25858.1","Tamron 24-135mm F3.5-5.6"}, // # (Model 190D)
				{"25881","Minolta AF 70-210mm F3.5-4.5"},
				{"25891","Minolta AF 80-200mm F2.8 APO or Tokina Lens"}, // # black
				{"25891.1","Tokina 80-200mm F2.8"},
				//25901 - Note: only get this with older 1.4x and lenses with 5-digit LensTypes (ref 27)
				//25901 - also "Minolta AF 200mm F2.8 HS-APO G + Minolta AF 1.4x APO"
				{"25901","Minolta AF 200mm F2.8 G APO + Minolta AF 1.4x APO or Other Lens + 1.4x"}, // #26
				{"25901.1","Minolta AF 600mm F4 HS-APO G + Minolta AF 1.4x APO"}, // #27
				{"25911","Minolta AF 35mm F1.4"}, // #(from Sony list) (not G as per ref 13)
				{"25921","Minolta AF 85mm F1.4 G (D)"},
				{"25931","Minolta AF 200mm F2.8 APO"}, // # (not "G", see 26121 - ref JR)
				{"25941","Minolta AF 3x-1x F1.7-2.8 Macro"},
				{"25961","Minolta AF 28mm F2"},
				{"25971","Minolta AF 35mm F2 [New]"}, // #13
				{"25981","Minolta AF 100mm F2"},
				//26011 - Note: only get this with older 2x and lenses with 5-digit LensTypes (ref 27)
				//# 26011 - also "Minolta AF 200mm F2.8 HS-APO G + Minolta AF 2x APO"
				{"26011","Minolta AF 200mm F2.8 G APO + Minolta AF 2x APO or Other Lens + 2x"}, // #26
				{"26011.1","Minolta AF 600mm F4 HS-APO G + Minolta AF 2x APO"}, // #27
				{"26041","Minolta AF 80-200mm F4.5-5.6"},
				{"26051","Minolta AF 35-80mm F4-5.6"}, // #(from Sony list)
				{"26061","Minolta AF 100-300mm F4.5-5.6"}, // # not (D) (ref 13/18)
				{"26071","Minolta AF 35-80mm F4-5.6"}, // #13
				{"26081","Minolta AF 300mm F2.8 HS-APO G"}, // # HS-APO added (ref 13/18)
				{"26091","Minolta AF 600mm F4 HS-APO G"},
				{"26121","Minolta AF 200mm F2.8 HS-APO G"},
				{"26131","Minolta AF 50mm F1.7 New"},
				{"26151","Minolta AF 28-105mm F3.5-4.5 xi"}, // # xi, not Power Zoom (ref 13/18)
				{"26161","Minolta AF 35-200mm F4.5-5.6 xi"}, // # xi, not Power Zoom (ref 13/18)
				{"26181","Minolta AF 28-80mm F4-5.6 xi"}, // # xi, not Power Zoom (ref 13/18)
				{"26191","Minolta AF 80-200mm F4.5-5.6 xi"}, // # xi, not Power Zoom (ref 13/18)
				{"26201","Minolta AF 28-70mm F2.8 G"}, // #11
				{"26211","Minolta AF 100-300mm F4.5-5.6 xi"}, // # xi, not Power Zoom (ref 13/18)
				{"26241","Minolta AF 35-80mm F4-5.6 Power Zoom"},
				{"26281","Minolta AF 80-200mm F2.8 HS-APO G"}, // #11 ("HS-APO" added, white, probably same as 1, non-HS is 25891 - ref JR)
				{"26291","Minolta AF 85mm F1.4 New"},
				{"26311","Minolta AF 100-300mm F4.5-5.6 APO"}, // #11 (does not exist? https://www.dyxum.com/dforum/lens-data-requested_topic23435_page2.html)
				{"26321","Minolta AF 24-50mm F4 New"},
				{"26381","Minolta AF 50mm F2.8 Macro New"},
				{"26391","Minolta AF 100mm F2.8 Macro"},
				{"26411","Minolta/Sony AF 20mm F2.8 New"}, // # Sony added (ref 13)
				{"26421","Minolta AF 24mm F2.8 New"},
				{"26441","Minolta AF 100-400mm F4.5-6.7 APO"}, // #11
				{"26621","Minolta AF 50mm F1.4 New"},
				{"26671","Minolta AF 35mm F2 New"},
				{"26681","Minolta AF 28mm F2 New"},
				{"26721","Minolta AF 24-105mm F3.5-4.5 (D)"}, // #11
				{"30464","Metabones Canon EF Speed Booster"}, // #Metabones (to this, add Canon LensType)
				{"45671","Tokina 70-210mm F4-5.6"}, // #22
				{"45681","Tokina AF 35-200mm F4-5.6 Zoom SD"}, // #IB (model 352)
				{"45701","Tamron AF 35-135mm F3.5-4.5"}, // #IB (model 40d)
				{"45711","Vivitar 70-210mm F4.5-5.6"}, // #IB
				{"45741","2x Teleconverter or Tamron or Tokina Lens"}, // #18
				{"45751","1.4x Teleconverter"}, // #18
				{"45851","Tamron SP AF 300mm F2.8 LD IF"}, // #11
				{"45861","Tamron SP AF 35-105mm F2.8 LD Aspherical IF"}, // #Fredrik Agert
				{"45871","Tamron AF 70-210mm F2.8 SP LD"}, // #Fabio Suprani
				// 48128: the Speed Booster Ultra appears to report type 48128 (=0xbc00)
				// - this is the base to which the Canon LensType is added
				{"48128","Metabones Canon EF Speed Booster Ultra"}, // #JR (to this, add Canon LensType)
				// 61184: older firmware versions of both the Speed Booster and the Smart Adapter
				//  report type 61184 (=0xef00), and add only the lower byte of the Canon LensType (ref JR).
				//For newer firmware versions this is only used by the Smart Adapter, and
				//the full Canon LensType code is added - PH
				//the metabones adapter translates Canon L -> G, II -> II, USM -> SSM, IS -> OSS (ref JR)
				//This offset is used by Metabones, Fotodiox, Sigma MC-11 EF-E and Viltrox Canon EF adapters.
				{"61184","Canon EF Adapter"}, // #JR (to this, add Canon LensType)
				// 65280 = 0xff00
				{"65280","Sigma 16mm F2.8 Filtermatic Fisheye"}, // #IB
				// all M42-type lenses give a value of 65535 (and FocalLength=0, FNumber=1)
				{"65535","E-Mount, T-Mount, Other Lens or no lens"}, // #JD/JR
			};

		public static string GetById(string id)
		{
			var idKeyValue = SonyIdDict.FirstOrDefault(p => p.Key == id);
			return idKeyValue.Value;
		}
	}
}
