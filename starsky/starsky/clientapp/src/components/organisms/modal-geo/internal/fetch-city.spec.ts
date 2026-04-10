import * as FetchGet from "../../../../shared/fetch/fetch-get";
import { fetchCity, getAltText } from "./fetch-city";
describe("getAltText", () => {
  it("returns empty string if neither province nor countryName", () => {
    expect(getAltText({ name: "A", latitude: 1, longitude: 2 })).toBe("");
  });
  it("returns province if only province is present", () => {
    expect(getAltText({ name: "A", latitude: 1, longitude: 2, province: "Noord-Holland" })).toBe(
      "Noord-Holland"
    );
  });
  it("returns countryName if only countryName is present", () => {
    expect(getAltText({ name: "A", latitude: 1, longitude: 2, countryName: "Netherlands" })).toBe(
      "Netherlands"
    );
  });
  it("returns 'province, countryName' if both are present", () => {
    expect(
      getAltText({
        name: "A",
        latitude: 1,
        longitude: 2,
        province: "Noord-Holland",
        countryName: "Netherlands"
      })
    ).toBe("Noord-Holland, Netherlands");
  });
});

describe("fetchCity", () => {
  afterEach(() => {
    jest.restoreAllMocks();
  });

  it("returns empty array if statusCode is not 200", async () => {
    jest.spyOn(FetchGet, "default").mockResolvedValueOnce({ statusCode: 404, data: null });
    const result = await fetchCity("Amsterdam");
    expect(result).toEqual([]);
  });

  it("maps city data to DropdownResult[]", async () => {
    const cityData = [
      { name: "Amsterdam", latitude: 52.37, longitude: 4.89 },
      { name: "Rotterdam", latitude: 51.92, longitude: 4.48 }
    ];
    jest.spyOn(FetchGet, "default").mockResolvedValueOnce({ statusCode: 200, data: cityData });
    const result = await fetchCity("Ams");
    expect(result).toEqual([
      { id: "52.37,4.89", displayName: "Amsterdam", altText: "" },
      { id: "51.92,4.48", displayName: "Rotterdam", altText: "" }
    ]);
  });

  it("returns empty array on fetch error", async () => {
    jest.spyOn(FetchGet, "default").mockRejectedValueOnce(new Error("Network error"));
    const result = await fetchCity("fail");
    expect(result).toEqual([]);
  });
});
