export interface INominatimAddress {
  road?: string;
  house_number?: string;
  suburb?: string;
  city?: string;
  town?: string;
  village?: string;
  state?: string;
  postcode?: string;
  country?: string;
}

export interface INominatimResponse {
  display_name: string;
  address: INominatimAddress;
}

/**
 * Fetches address information from Nominatim reverse geocoding API
 * @param latitude - Latitude coordinate
 * @param longitude - Longitude coordinate
 * @returns Promise with address details or null if failed
 */
export async function FetchAddressFromNominatim(
  latitude: number,
  longitude: number
): Promise<INominatimResponse | null> {
  try {
    const url = `https://nominatim.openstreetmap.org/reverse?format=json&lat=${latitude}&lon=${longitude}&addressdetails=1`;

    const response = await fetch(url, {
      headers: {
        "User-Agent": "Starsky-App"
      }
    });

    if (!response.ok) {
      console.error("Nominatim API request failed:", response.statusText);
      return null;
    }

    const data = await response.json();
    return data as INominatimResponse;
  } catch (error) {
    console.error("Error fetching address from Nominatim:", error);
    return null;
  }
}

/**
 * Extracts street name from Nominatim address
 * @param address - Nominatim address object
 * @returns Street name with house number if available
 */
export function GetStreetName(address: INominatimAddress): string {
  if (!address.road) {
    return "";
  }

  if (address.house_number) {
    return `${address.road} ${address.house_number}`;
  }

  return address.road;
}
