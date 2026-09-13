import { UrlQuery } from "../../../../shared/url/url-query";
import {
  FetchMapElementsFromOsm,
  IOsmMapElementsResult
} from "./fetch-osm-map-elements";

describe("FetchMapElementsFromOsm", () => {
  beforeEach(() => {
    global.fetch = jest.fn();
  });

  afterEach(() => {
    jest.resetAllMocks();
  });

  it("should fetch map elements successfully", async () => {
    const mockResponse: IOsmMapElementsResult = {
      nearbyObjects: [{ id: "node/1", label: "Cafe Central" }],
      enclosingObjects: [{ id: "area/2", label: "Amsterdam" }]
    };

    (global.fetch as jest.Mock).mockResolvedValueOnce({
      ok: true,
      json: async () => mockResponse,
      status: 200,
      data: mockResponse
    });

    const result = await FetchMapElementsFromOsm(52.52, 13.405);

    expect(result).toEqual(mockResponse);
    expect(global.fetch).toHaveBeenCalledWith(
      new UrlQuery().UrlGeoMapElements(52.52, 13.405),
      expect.objectContaining({
        credentials: "include",
        headers: {
          Accept: "application/json",
          "User-Agent": "Starsky-App",
          "X-Requested-With": "XMLHttpRequest"
        },
        method: "GET"
      })
    );
  });

  it("should return null when API request fails", async () => {
    (global.fetch as jest.Mock).mockResolvedValueOnce({
      ok: false,
      status: 404,
      statusText: "Not Found"
    });

    const result = await FetchMapElementsFromOsm(0, 0);

    expect(result).toBeNull();
  });

  it("should return null when fetch throws error", async () => {
    (global.fetch as jest.Mock).mockRejectedValueOnce(new Error("Network error"));

    const result = await FetchMapElementsFromOsm(52.52, 13.405);

    expect(result).toBeNull();
  });
});
