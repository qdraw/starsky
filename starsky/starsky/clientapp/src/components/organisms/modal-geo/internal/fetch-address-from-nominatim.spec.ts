import {
  FetchAddressFromNominatim,
  GetStreetName,
  INominatimAddress
} from "./fetch-address-from-nominatim";

describe("FetchAddressFromNominatim", () => {
  beforeEach(() => {
    global.fetch = jest.fn();
  });

  afterEach(() => {
    jest.resetAllMocks();
  });

  it("should fetch address successfully", async () => {
    const mockResponse = {
      display_name: "Test Street 123, Test City",
      address: {
        road: "Test Street",
        house_number: "123",
        city: "Test City",
        country: "Test Country"
      }
    };

    (global.fetch as jest.Mock).mockResolvedValueOnce({
      ok: true,
      json: async () => mockResponse,
      statusCode: 200,
      data: mockResponse
    });

    const result = await FetchAddressFromNominatim(52.52, 13.405);

    expect(result).toEqual(mockResponse);
    expect(global.fetch).toHaveBeenCalledWith(
      expect.stringContaining("https://nominatim.openstreetmap.org/reverse"),
      expect.objectContaining({
        headers: {
          "User-Agent": "Starsky-App"
        }
      })
    );
  });

  it("should return null when API request fails", async () => {
    (global.fetch as jest.Mock).mockResolvedValueOnce({
      ok: false,
      statusText: "Not Found"
    });

    const result = await FetchAddressFromNominatim(0, 0);

    expect(result).toBeNull();
  });

  it("should return null when fetch throws error", async () => {
    (global.fetch as jest.Mock).mockRejectedValueOnce(new Error("Network error"));

    const result = await FetchAddressFromNominatim(52.52, 13.405);

    expect(result).toBeNull();
  });

  it("should include correct coordinates in URL", async () => {
    (global.fetch as jest.Mock).mockResolvedValueOnce({
      ok: true,
      json: async () => ({ display_name: "", address: {} })
    });

    await FetchAddressFromNominatim(52.52, 13.405);

    expect(global.fetch).toHaveBeenCalledWith(
      "https://nominatim.openstreetmap.org/reverse?format=json&lat=52.52&lon=13.405&addressdetails=1",
      expect.any(Object)
    );
  });
});

describe("GetStreetName", () => {
  it("should return street name with house number", () => {
    const address: INominatimAddress = {
      road: "Main Street",
      house_number: "42"
    };

    const result = GetStreetName(address);

    expect(result).toBe("Main Street 42");
  });

  it("should return street name without house number", () => {
    const address: INominatimAddress = {
      road: "Main Street"
    };

    const result = GetStreetName(address);

    expect(result).toBe("Main Street");
  });

  it("should return empty string when no road in address", () => {
    const address: INominatimAddress = {
      city: "Test City"
    };

    const result = GetStreetName(address);

    expect(result).toBe("");
  });

  it("should return empty string for empty address", () => {
    const address: INominatimAddress = {};

    const result = GetStreetName(address);

    expect(result).toBe("");
  });
});
