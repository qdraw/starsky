import * as FetchGet from "../../../../shared/fetch/fetch-get";
import * as UrlQuery from "../../../../shared/url/url-query";
import { fetchCityTimezones } from "./fetch-city-timezones";

describe("fetchCityTimezones", () => {
  const mockDateTime = "2024-08-12";
  const mockCity = "New York";

  afterEach(() => {
    jest.clearAllMocks();
    jest.restoreAllMocks();
  });

  it("returns dropdown results on successful 200 response", async () => {
    const mockResults = [
      {
        id: "America/New_York",
        displayName: "(UTC-04:00) New York"
      },
      {
        id: "America/Chicago",
        displayName: "(UTC-05:00) Chicago"
      }
    ];

    jest.spyOn(FetchGet, "default").mockResolvedValue({
      statusCode: 200,
      data: mockResults
    });

    const result = await fetchCityTimezones(mockDateTime, mockCity);

    expect(result).toEqual(mockResults);
    expect(FetchGet.default).toHaveBeenCalledWith(
      new UrlQuery.UrlQuery().UrlGeoLocationNameCityTimezone(mockDateTime, mockCity)
    );
  });

  describe.each([
    { statusCode: 400, description: "400 Bad Request" },
    { statusCode: 500, description: "500 Internal Server Error" },
    { statusCode: 502, description: "502 Bad Gateway" },
    { statusCode: 503, description: "503 Service Unavailable" }
  ])("returns empty array on $statusCode status code", ({ statusCode }) => {
    it(`returns empty array for ${statusCode}`, async () => {
      jest.spyOn(FetchGet, "default").mockResolvedValue({
        statusCode,
        data: null
      });

      const result = await fetchCityTimezones(mockDateTime, mockCity);

      expect(result).toEqual([]);
    });
  });

  it("calls UrlQuery with correct datetime and city parameters", async () => {
    const mockUrlSpy = jest.fn().mockReturnValue("/api/geo-location");
    jest.spyOn(UrlQuery, "UrlQuery").mockImplementation(
      () =>
        ({
          UrlGeoLocationNameCityTimezone: mockUrlSpy
        }) as unknown as UrlQuery.UrlQuery
    );

    jest.spyOn(FetchGet, "default").mockResolvedValue({
      statusCode: 200,
      data: []
    });

    await fetchCityTimezones(mockDateTime, mockCity);

    expect(mockUrlSpy).toHaveBeenCalledWith(mockDateTime, mockCity);
  });

  it("returns empty array when response data is empty", async () => {
    jest.spyOn(FetchGet, "default").mockResolvedValue({
      statusCode: 200,
      data: []
    });

    const result = await fetchCityTimezones(mockDateTime, mockCity);

    expect(result).toEqual([]);
  });

  it("handles FetchGet throwing an error", async () => {
    jest.spyOn(FetchGet, "default").mockRejectedValue(new Error("Network error"));

    await expect(fetchCityTimezones(mockDateTime, mockCity)).rejects.toThrow("Network error");
  });
});
