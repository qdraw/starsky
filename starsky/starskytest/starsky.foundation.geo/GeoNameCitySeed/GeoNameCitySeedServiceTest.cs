using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.GeoNamesCities;
using starsky.foundation.geo.GeoNameCitySeed;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;
using VerifyMSTest;

namespace starskytest.starsky.foundation.geo.GeoNameCitySeed;

[TestClass]
public sealed class GeoNameCitySeedServiceTest : VerifyBase
{
	private static readonly string[] SampleLines =
	[
		"038832	Vila	Vila	Casas Vila,Vila	42.53176	1.56654	P	PPL	AD		03				1418		1318	Europe/Andorra	2024-11-04",
		"3039154	El Tarter	El Tarter	Ehl Tarter,El Tarter,El Tarter - Principau d'Andorra,El Tarter - Principáu d'Andorra,al tartr,Ел Тартер,Эл Тартер,ال تارتر	42.57952	1.65362	P	PPL	AD		02				1052		1721	Europe/Andorra	2012-11-03",
		"3039163	Sant Julià de Lòria	Sant Julia de Loria	San Julia,San Julià,Sant Julia de Loria,Sant Julià de Lòria,Sant-Zhulija-de-Lorija,sheng hu li ya-de luo li ya,Сант-Жулия-де-Лория,サン・ジュリア・デ・ロリア教区,圣胡利娅-德洛里亚,圣胡利娅－德洛里亚	42.46372	1.49129	P	PPLA	AD		06				8022		921	Europe/Andorra	2013-11-23",
		"3039604	Pas de la Casa	Pas de la Casa	Pas de la Kasa,Пас де ла Каса	42.54277	1.73361	P	PPL	AD		03				2363	2050	2106	Europe/Andorra	2008-06-09",
		"3039678	Ordino	Ordino	Ordino,ao er di nuo,orudino jiao qu,Ордино,オルディノ教区,奥尔迪诺	42.55623	1.53319	P	PPLA	AD		05				3066		1296	Europe/Andorra	2018-10-26",
		"3040051	les Escaldes	les Escaldes	Ehskal'des-Ehndzhordani,Escaldes,Escaldes-Engordany,Les Escaldes,esukarudesu=engorudani jiao qu,lai sai si ka er de-en ge er da,Эскальдес-Энджордани,エスカルデス＝エンゴルダニ教区,萊塞斯卡爾德-恩戈爾達,萊塞斯卡爾德－恩戈爾達	42.50729	1.53414	P	PPLA	AD		08				15853		1033	Europe/Andorra	2024-06-20",
		"3040132	la Massana	la Massana	La Macana,La Massana,La Maçana,La-Massana,la Massana,ma sa na,Ла-Массана,ラ・マサナ教区,马萨纳	42.54499	1.51483	P	PPLA	AD		04				7211		1245	Europe/Andorra	2008-10-15",
		"3040686	Encamp	Encamp	Ehnkam,Encamp,en kan pu,enkanpu jiao qu,Энкам,エンカンプ教区,恩坎普	42.53474	1.58014	P	PPLA	AD		03				11223		1257	Europe/Andorra	2018-10-26",
		"3041204	Canillo	Canillo	Canillo,Kanil'o,ka ni e,kaniryo jiao qu,Канильо,カニーリョ教区,卡尼略	42.5676	1.59756	P	PPLA	AD		02				3292		1561	Europe/Andorra	2018-10-26",
		"3041519	Arinsal	Arinsal	Arinsal,Arinsal',Arinsalis,arynsal,Аринсал,Аринсаль,آرینسال	42.57205	1.48453	P	PPL	AD		04				1419		1465	Europe/Andorra	2010-01-29",
		"3041543	Anyós	Anyos	Anyos,Anyós	42.53465	1.5251	P	PPL	AD		04				1006		1299	Europe/Andorra	2024-11-04",
		"3041563	Andorra la Vella	Andorra la Vella	ALV,Ando-la-Vyey,Andora,Andora la Vela,Andora la Velja,Andora lja Vehl'ja,Andoro Malnova,Andorra,Andorra Tuan,Andorra a Vella,Andorra la Biella,Andorra la Vella,Andorra la Vielha,Andorra-a-Velha,Andorra-la-Vel'ja,Andorra-la-Vielye,Andorre-la-Vieille,Andò-la-Vyèy,Andòrra la Vièlha,an dao er cheng,andolalabeya,andwra la fyla,Ανδόρρα,Андора ла Веля,Андора ла Веља,Андора ля Вэлья,Андорра-ла-Велья,אנדורה לה וולה,أندورا لا فيلا,አንዶራ ላ ቬላ,アンドラ・ラ・ヴェリャ,安道爾城,안도라라베야	42.50779	1.52109	P	PPLC	AD		07				20430		1037	Europe/Andorra	2020-03-03",
		"3041604	Aixirivall	Aixirivall	Aixirivali,Aixirivall,Aixirvall,Eixirivall	42.46245	1.50209	P	PPL	AD		06				1041		1169	Europe/Andorra	2024-11-04",
		"290503	Warīsān	Warisan	Warisan,Warsan,Warīsān,wrsan,ورسان	25.16744	55.40708	P	PPL	AE		03				108759		12	Asia/Dubai	2024-06-11",
		"290581	Umm Suqaym	Umm Suqaym	Umm Suqaym,Umm Suqeim,Umm Suqeim 2,Umm as Suqaym,am sqym,أم سقيم	25.15491	55.21015	P	PPLX	AE		03				16459		1	Asia/Dubai	2024-10-28",
		"290594	Umm Al Quwain City	Umm Al Quwain City	Oumm al Qaiwain,Oumm al Qaïwaïn,Um al Kawain,Um al Quweim,Umm Al Quwain City,Umm al Qaiwain,Umm al Qawain,Umm al Qaywayn,Umm al-Quwain,Umm-ehl'-Kajvajn,Yumul al Quwain,am alqywyn,mdynt am alqywyn,Умм-эль-Кайвайн,أم القيوين,مدينة ام القيوين	25.56473	55.55517	P	PPLA	AE		07				59098		2	Asia/Dubai	2025-04-17",
		"290680	Ţarīf Kalbā	Tarif Kalba	Tarif Kalba,Ţarīf Kalbā	25.0695	56.33115	P	PPL	AE		04				51000		10	Asia/Dubai	2025-04-17",
		"291061	Ar Rāshidīyah	Ar Rashidiyah	Al Rashidiya,Ar Rashidiyah,Ar Rāshidīyah,Rashdia,Rashidiyah,Rashīdīyah,alrashdyt,الراشدية	25.22499	55.38947	P	PPLX	AE		03				38425		10	Asia/Dubai	2024-10-28",
		"291074	Ras Al Khaimah	Ras Al Khaimah	Julfa,Khaimah,RAK City,RKT,Ra's al Khaymah,Ra's al-Chaima,Ras Al Khaimah City,Ras al Hajma,Ras al Khaimah,Ras al-Chajma,Ras al-Hajma,Ras al-Khaimah,Ras el Khaimah,Ras el Khajma,Ras el Khaïmah,Ras el-Kheima,Ras Əl-Xayma,Ras-ehl'-Khajma,Ras-el'-Khajma,Ra’s al Khaymah,Ra’s al-Chaima,Resue'l-Hayme,Resü'l-Hayme,ras ٱlkhaymat,rʼs ʼl-h'ymh,Рас ел Хајма,Рас-ель-Хайма,Рас-эль-Хайма,ראס אל-ח'ימה,رَأْس ٱلْخَيْمَة	25.78953	55.9432	P	PPLA	AE		05				351943		2	Asia/Dubai	2025-11-16",
		"291279	Muzayri‘	Muzayri'	Mezaira'a,Mezaira’a,Mizeir`ah,Mizeir‘ah,Mozayri`,Mozayri‘,Muzairi,Muzayri`,Muzayri‘,Музаири	23.14355	53.7881	P	PPL	AE		01	103	12747983		10000		123	Asia/Dubai	2024-03-14",
		"291339	Murbaḩ	Murbah	Marbah,Mirba,Mirbih,Mirbiḥ,Murbah,Murbaḩ,Мирба	25.27623	56.36256	P	PPL	AE		06				2000		15	Asia/Dubai	2020-06-10",
		"291489	Maşfūţ	Masfut	Masfut,Maşfūţ,Sha'biyyat Masfut,Sha'biyyāt Maşfūt	24.81089	56.10657	P	PPL	AE		02				8988	460	334	Asia/Dubai	2024-10-07",
		"291580	Zayed City	Zayed City	Bid' Zayed,Bid’ Zayed,Madinat Za'id,Madinat Zayid,Madīnat Zāyid,Madīnat Zā’id,Zayed City,mdynt zayd,مدينة زايد	23.65416	53.70522	P	PPL	AE		01	103	12748055		63482		118	Asia/Dubai	2024-03-14",
		"291696	Khawr Fakkān	Khawr Fakkan	Fakkan,Fakkān,Khawr Fakkan,Khawr Fakkān,Khawr al Fakkan,Khawr al Fakkān,Khor Fakhan,Khor Fakkan,Khor Fakkan City,Khor Fakkān,Khor al Fakhan,Khor al Fakkan,Khor al Fākhān,Khor'fakkan,Khor-Fakkan,Khorfakan,Khorfakhan,Port Khor Fakkan,mdynt khwr fkan,Хор-Факкан,مدينة خور فكان	25.33132	56.34199	P	PPL	AE		06				40677		20	Asia/Dubai	2024-09-12",
		"291763	Kalbā	Kalba	Ghalla,Ghallah,Ghālla,Ghāllah,Kalba,Kalbah,Kalbā,Qalba	25.0513	56.35422	P	PPL	AE		06				37545		15	Asia/Dubai	2024-10-17",
		"291775	Jumayrā	Jumayra	Jimeirah,Jumaira,Jumairah,Jumayra,Jumayrah,Jumayrā,Jumeira,Jumeirah,jmyra,جميرا	25.20795	55.24969	P	PPLX	AE		03				39080		5	Asia/Dubai	2024-04-03",
		"291794	Al Jazīrah al Ḩamrā’	Al Jazirah al Hamra'	Al Hamra',Al Jazeera Al Hamra,Al Jazirah al Hamra',Al Jazīrah al Ḩamrā’,Al Ḩamrā’,Hamra	25.7091	55.80772	P	PPL	AE		05				10190		3	Asia/Dubai	2024-10-28",
		"292223	Dubai	Dubai	DXB,Dabei,Dibai,Dibay,Doubayi,Dubae,Dubai,Dubai City,Dubai emiraat,Dubaija,Dubaj,Dubajo,Dubajus,Dubay,Dubayy,Dubaï,Dubái,Dúbæ,Fort Dabei,Ntoumpai,dby,dbyy,di bai,dobai,du bai,duba'i,dubai,dubay,dubi,dwbyy,tupai,Ντουμπάι,Дубаи,Дубай,Դուբայ,דובאי,דוביי,دبئی,دبى,دبي,دبی,دوبەی,دۇبائى,दुबई,দুবাই,துபை,దుబాయ్,ದುಬೈ,ദുബായ്,ดูไบ,დუბაი,ドバイ,杜拜,迪拜,두바이	25.07725	55.30927	P	PPLA	AE		03				3790000		24	Asia/Dubai	2025-04-17",
		"292231	Dibba Al-Fujairah	Dibba Al-Fujairah	Al-Fujairah,BYB,Dibba Al-Fujairah,dba alfjyrt,دبا الفجيرة	25.59246	56.26176	P	PPL	AE		04				30000		16	Asia/Dubai	2014-08-12",
		"292239	Dibba Al-Hisn	Dibba Al-Hisn	BYB,Daba,Daba al-Hisn,Dabā,Dabā al-Ḥiṣn,Diba,Diba Al Hisn,Diba al Hisn,Dibah,Dibba,Dibba Al Hisn,Dibba Al'-Khisn,Dibba Al-Hisn,Dibbah,Dibbā Al Ḩişn,Dibā,Dibā Al Ḩişn,Dībā al Ḩişn,Hisn Diba,Husn Dibba,Дибба Аль-Хисн,Ḩişn Dibā	25.61955	56.27291	P	PPL	AE		04				26395		4	Asia/Dubai	2024-07-14",
		"292261	Dayrah	Dayrah	Daira,Dairah,Dayrah,Deira,Dirah,Dīrah,dyrt,ديرة	25.27143	55.30207	P	PPLX	AE		03				400000		10	Asia/Dubai	2024-10-28",
		"292672	Sharjah	Sharjah	Al Sharjah,Ash 'Mariqah,Ash Shariqa,Ash Shariqah,Ash Shāriqa,Ash Shāriqah,Ash ’Mariqah,Ash-Shariqah emiraat,Ash-Shāriqah emiraat,Charjah,Ch·ardj·a,SHJ,Sardza,Sardzsa,Sarika,Sarja,Sarjo,Sarza,Schardscha,Shardza,Shardzha,Shardzha kuorat,Sharga,Sharijah,Shariqah,Sharja,Sharjah,Sharjah city,Shārijah,Shāriqah,Shārja,Szardza,Szardża,Xarja,Xarjah,alsharqt,amart alsharqt,carja,charc ah,mdynt alsharqt,saraja,sarajaha,sarja,sharja,sharjh,sharuja,syaleuja,sʼrgh,xia er jia,Ŝarĵo,Şarika,Şarja,Šardža,Šardžá,Шарджа,Шарджа куорат,Шарџа,Шарҗә,Շարժա,שארגה,إمارة الشارقة,الشارقة,شارجه,شارجہ,مدينة الشارقة,शारजा,शारजाह,ਸ਼ਾਰਜਾ,சார்ஜா,షార్జా,ಶಾರ್ಜ,ഷാർജ,ชาร์จาห์,შარჯა,シャールジャ,夏尔迦,샤르자	25.3342	55.41221	P	PPLA	AE		06				1800000		15	Asia/Dubai	2025-11-13",
		"292674	Ash Sha‘m	Ash Sha'm	Ash Sha`m,Ash Sha‘m,Sha'am,Sha`m,Shaam,Sha‘m	26.0279	56.08352	P	PPL	AE		05				1550		5	Asia/Dubai	2024-10-28",
		"292688	Ar Ruways	Ar Ruways	Ar Ru'ays,Ar Ruways,Ar Ru’ays,Ar-Ruvais,Ruwais,alrwys,ar-Ruways,lu wa si,ruvais,Ар-Руваис,الرويس,الرویس,റുവൈസ്,魯瓦斯,鲁瓦斯	24.11028	52.73056	P	PPL	AE		01	103	12748114		25000		16	Asia/Dubai	2024-03-14",
		"292799	Al Manāmah	Al Manamah	Al Manamah,Al Manāmah,Manama,Manamah,Manāmah	25.3299	56.02188	P	PPL	AE		02				5823		213	Asia/Dubai	2022-01-13",
		"292856	Al Ḩamrīyah	Al Hamriyah	Al Hamriya,Al Hamriyah,Al Ḩamrīyah,Hamriya,Hamriyah,Hamriyyah,alhmryt,الحمرية,Ḥamriyyah,Ḩamrīyah	25.47819	55.53377	P	PPL	AE		06				3297		8	Asia/Dubai	2024-04-03",
		"292862	Ḩattā	Hatta	Al Hagarein,Al Hajarayn,Al Ḥagarein,Al Ḩajarayn,Hajarain,Hajarayn,Hajrain,Hatta,hta,حتا,Ḩajarayn,Ḩattā	24.80073	56.12726	P	PPL	AE		03	800	891		15324	335	331	Asia/Dubai	2024-10-03",
		"292878	Fujairah	Fujairah	Al Fujairah City,Al Fujayrah,Al-Fudjayra,Al-Fujayrah' emiraat,El'-Fudzhajra,FJR,Fudschaira,Fudzajra,Fudzejra,Fudzhejra,Fudżajra,Fudžajra,Fueceyre,Fueceyrə,Fujaira,Fujairah,Fujajro,Fujayrah,Fuĵajro,Füceyre,Füceyrə,alfjyrt,fjyrt,fu ji la,fujaira,mdynt alfjyrt,pwg'yyrh,Ель-Фуджайра,Фуджейра,Фуџејра,פוג'יירה,الفجيرة,فجيرة,مدينة الفجيرة,フジャイラ,富吉拉	25.11641	56.34141	P	PPLA	AE		04				118933		15	Asia/Dubai	2025-11-15",
		"292913	Al Ain City	Al Ain City	AAN,Ainas,Al Ain,Al Ain City,Al Ajn,Al Ayn,Al `Ayn,Al Ɛayn,Al ‘Ayn,Al-Ain,Al-Ajn,Al-Ayin,Al-Ayn,Al-Aïn,Ehl'-Ajn,El Ain,El-Ajn,ai yin,al ain,al-ain,al-aini,alʿyn,ela ena,mdynt alʿyn,Ел Аин,Эль-Айн,Ալ-Ային,אל-עין,العين,العین,مدينة العين,एल एन,அல் ஐன்,അൽ ഐൻ,ალ-აინი,アル・アイン,艾因,알아인	24.19167	55.76056	P	PPL	AE		01	102			846747		275	Asia/Dubai	2024-07-13",
		"292932	Ajman	Ajman	Acman,Adschman,Adzhman,Adzman,Adżman,Adžman,Ajman,Ajman City,Al Ajman,QAJ,Ujman,Əcmən,ʻg'mʼn,ʿjman,ʿyman,Аджман,Аџман,עג'מאן,عجمان,عيمان	25.40177	55.47878	P	PPLA	AE		02				490035		2	Asia/Dubai	2025-11-14",
		"292953	Adh Dhayd	Adh Dhayd	Adh Dhaid,Adh Dhayd,Adh Dhed,Adh Dhēd,Al Daid,Al-Dhayd,Az-Zajd,Dayd,Dhaid,Dhayd,Duhayd,Ehd-Dajd,Ihaid,Zeyd,adh-Dhayd,aldhyd,da yi de,daid,dhyd,Эд-Дайд,الذيد,ذید,ദൈദ്,Ḑayd,達伊德	25.28812	55.88157	P	PPL	AE		06				20165		114	Asia/Dubai	2025-01-05",
		"292968	Abu Dhabi	Abu Dhabi	A-pu-that-pi,AEbu Saby,AUH,Aboe Dhabi,Abou Dabi,Abu Dabi,Abu Dabis,Abu Daby,Abu Daibi,Abu Dhabi,Abu Dhabi Island and Internal Islands City,Abu Dhabi emiraat,Abu Zabi,Abu Zaby,Abu Zabye,Abu Zabyo,Abu Ḍabi,Abu Ḑabi,Abu-Dabi,Abu-Dabi khot,Abu-Dabio,Abu-Dzabi,Abú Dabí,Abú Daibí,Abú Zabí,Abû Daby,Abū Dabī,Abū Z̧aby,Abū Z̧abye,Abū Z̧abyo,Abū Z̧abī,Ampou Ntampi,Ebu Dabi,Ebu Dhabi,a bu zha bi,abu dhabi,abu-dabi,abudabi,abudhabi,abw zby,abwzby,aputapi,jzyrt abwzby wjzr dakhlyt akhry,xa bud abi,Â-pu-tha̍t-pí,Äbu Saby,Əbu-Dabi,Άμπου Ντάμπι,Αμπου Νταμπι,Αμπού Ντάμπι,Абу Даби,Абу-Даби,Абу-Даби хот,Абу-Дабі,Әбу-Даби,Աբու Դաբի,אבו דאבי,أبوظبي,ئەبووزەبی,ابو ظبى,ابوظبی,ابوظہبی,جزيرة أبوظبي وجزر داخلية اخرى,अबु धाबी,अबू धाबी,আবুধাবি,ਅਬੂ ਧਾਬੀ,ଆବୁଧାବି,அபுதாபி,ಅಬು ಧಾಬಿ,അബുദാബി,අබුඩාබි,อาบูดาบี,ཨ་པོའུ་དྷ་པེ།,အဘူဒါဘီမြို့,აბუ-დაბი,አቡ ዳቢ,アブダビ,阿布扎比,아부다비	24.45118	54.39696	P	PPLC	AE		01	101			1807000		6	Asia/Dubai	2024-03-27",
		"292991	Abū Hayl	Abu Hayl	Abu Hail,Hail,Hāil	25.28413	55.33153	P	PPLX	AE		03				18043		3	Asia/Dubai	2024-10-28",
		"385024	As Saţwah	As Satwah	Al Satwa,As Satwah,As Saţwah,Satwa,alstwh,السطوه	25.22192	55.27459	P	PPLX	AE		03				40997		9	Asia/Dubai	2024-10-28",
		"394142	Margham	Margham	Margham,Murgham,Sha'biyyat Murgham	24.89952	55.62545	P	PPL	AE		03	800	845		1280		192	Asia/Dubai	2025-10-25",
		"400747	Abū Mūsá	Abu Musa	Abu Musa,Abū Mūsá,abw mwsy,ابو موسیٰ	25.86698	55.02363	P	PPL	AE	AE,IR	11				4213		7	Asia/Dubai	2025-05-19",
		"412567	Nadd al Ḩumr	Nadd al Humr	Nad al Hamar,Nadd Al Hamar,Nadd al Hamar,Nadd al Humr,Nadd al Ḩamar,Nadd al Ḩumr,nd alhmr,ند الحمر	25.20131	55.38388	P	PPLX	AE		03				13589		10	Asia/Dubai	2024-10-28",
		"412981	Al Lusaylī	Al Lusayli	Al Lisaili,Al Lusayli,Al Lusaylī	24.93138	55.47531	P	PPL	AE		03				2514		116	Asia/Dubai	2024-10-28",
		"413256	Suwayḩān	Suwayhan	Suwayhan,Suwayḩān,Sweihan,suayhan,سُوَيْحَان	24.46235	55.33715	P	PPL	AE		01	102	12747987		5403		147	Asia/Dubai	2024-10-28",
		"414901	Al Ḩamīdīyah	Al Hamidiyah	Al Hamidiyah,Al Ḩamīdīyah,alhmydyt,الحميدية	25.40001	55.52925	P	PPLX	AE		02				27000		15	Asia/Dubai	2024-04-03",
		"6691079	Al Waheda	Al Waheda		25.29173	55.33822	P	PPLX	AE		03				21608		-1	Asia/Dubai	2024-10-28",
		"6691083	Al Twar First	Al Twar First	Al Twar 1	25.27148	55.36165	P	PPLX	AE		03				12114		2	Asia/Dubai	2024-10-28",
		"6691085	AL Twar Second	AL Twar Second	Al Twar 2	25.26309	55.38028	P	PPLX	AE		03				5068		4	Asia/Dubai	2024-10-28",
		"6691086	Al Qusais Second	Al Qusais Second	Al Qusais 2	25.27148	55.38603	P	PPLX	AE		03				12851		8	Asia/Dubai	2024-10-28",
		"6691091	Al Karama	Al Karama		25.24004	55.30106	P	PPLX	AE		03				75560		4	Asia/Dubai	2024-10-28"
	];

	[TestMethod]
	public async Task Seed_InsertsUniqueCities_Verify()
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase("GeoNameCitySeedServiceTestDb")
			.Options;
		using var dbContext = new ApplicationDbContext(options);
		var appSettings = new AppSettings { DependenciesFolder = "." };
		var fakeSelectorStorage = new FakeSelectorStorage(new SampleFakeIStorage());
		var fakeGeoFileDownload = new FakeIGeoFileDownload();
		var fakeLogger = new FakeIWebLogger();
		var fakeMemoryCache = new MemoryCache(new MemoryCacheOptions());
		var fakeScopeFactory = new FakeIServiceScopeFactory();
		var geoNamesCitiesQuery = new GeoNamesCitiesQuery(dbContext, fakeScopeFactory);
		var service = new GeoNameCitySeedService(
			fakeSelectorStorage,
			appSettings,
			fakeGeoFileDownload,
			geoNamesCitiesQuery,
			fakeLogger,
			fakeMemoryCache
		);

		var result = await service.Seed();
		Assert.IsTrue(result);

		var all = await dbContext.GeoNameCities.ToListAsync(TestContext.CancellationToken);
		Assert.HasCount(55, all);

		await Verify(all);

		Assert.IsTrue(all.Any(x => x.GeonameId == 38832 && x.Name == "Vila"));
		Assert.IsTrue(all.Any(x => x.GeonameId == 3039154 && x.Name == "El Tarter"));
		Assert.IsTrue(all.Any(x => x.GeonameId == 3039163 && x.Name.StartsWith("Sant Juli")));
		Assert.IsTrue(all.Any(x => x.GeonameId == 6691091 && x.Name.StartsWith("Al Karama")));
	}

	private class SampleFakeIStorage : FakeIStorage
	{
		public override IAsyncEnumerable<string> ReadLinesAsync(string path,
			CancellationToken cancellationToken)
		{
			return GetLines();

			async IAsyncEnumerable<string> GetLines()
			{
				foreach ( var line in SampleLines )
				{
					yield return line;
				}
			}
		}

		public override bool ExistFile(string path)
		{
			return true;
		}
	}
}
