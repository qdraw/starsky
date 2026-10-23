// Records live Overpass responses as test fixtures for OsmMapElementsFixtureTest.
//
//   node capture-fixtures.js
//
// The queries are the same as in OsmMapElementsService. Only the tags the service reads are kept
// to keep the files small. After a refresh the expectations in OsmMapElementsFixtureTest
// might need an update, because OpenStreetMap data changes over time.
const fs = require("node:fs");
const path = require("node:path");

const HOSTS = ["overpass-api.de", "z.overpass-api.de", "lz4.overpass-api.de"];
const KEEP_TAGS = new Set([
  "name", "official_name", "brand", "operator", "ref",
  "amenity", "shop", "tourism", "historic", "leisure", "man_made", "building",
  "highway", "waterway", "railway", "natural", "landuse", "boundary", "place"
]);

const PLACES = {
  "tete-de-porte": [46.046546, 6.752602],
  "lac-d-emosson": [46.08353, 6.912979],
  "worpplantsoen": [52.25044, 6.147579],
  "wilhelminabrug": [52.248773, 6.157151],
  "zoo-berlin": [52.508364, 13.338982],
  "hauptbahnhof-berlin": [52.525223, 13.370114],
  "brandenburger-tor": [52.5163, 13.3777],
  "matterhorn": [45.97642, 7.65853],
  "eiffeltoren": [48.85837, 2.29448]
};

const nearbyQuery = (lat, lon) =>
  `[timeout:10][out:json];(node(around:20,${lat},${lon});way(around:20,${lat},${lon});` +
  `relation(around:20,${lat},${lon}););out center tags qt;`;
const enclosingQuery = (lat, lon) =>
  `[timeout:10][out:json];is_in(${lat},${lon})->.a;(way(pivot.a);relation(pivot.a););out center tags qt;`;

const sleep = (ms) => new Promise((resolve) => setTimeout(resolve, ms));

async function query(data) {
  for (let attempt = 0; attempt < 4; attempt++) {
    for (const host of HOSTS) {
      try {
        const response = await fetch(`https://${host}/api/interpreter`, {
          method: "POST",
          headers: { "User-Agent": "Starsky-App", "Content-Type": "application/x-www-form-urlencoded" },
          body: `data=${encodeURIComponent(data)}`
        });
        const json = response.ok ? await response.json() : null;
        if (json && !json.remark) return json;
      } catch {
        // try next host
      }
      await sleep(2000);
    }
  }
  throw new Error("Overpass not available");
}

function slim(response) {
  const elements = response.elements.map((element) => {
    const tags = Object.fromEntries(
      Object.entries(element.tags ?? {}).filter(([key]) => KEEP_TAGS.has(key))
    );
    const slimmed = { type: element.type, id: element.id };
    if (element.lat !== undefined) { slimmed.lat = element.lat; slimmed.lon = element.lon; }
    if (element.center) slimmed.center = element.center;
    slimmed.tags = tags;
    return slimmed;
  });
  return JSON.stringify({ elements }, null, 1) + "\n";
}

(async () => {
  for (const [name, [lat, lon]] of Object.entries(PLACES)) {
    for (const [kind, build] of [["nearby", nearbyQuery], ["enclosing", enclosingQuery]]) {
      const file = path.join(__dirname, `${name}.${kind}.json`);
      fs.writeFileSync(file, slim(await query(build(lat, lon))));
      console.log(file);
      await sleep(3000);
    }
  }
})();
