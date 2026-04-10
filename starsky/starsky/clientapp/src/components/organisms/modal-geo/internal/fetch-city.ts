import FetchGet from "../../../../shared/fetch/fetch-get";
import { UrlQuery } from "../../../../shared/url/url-query";
import { DropdownResult } from "../../../atoms/searchable-dropdown/ISearableDropdownProps";

export const fetchCity = async (city: string): Promise<DropdownResult[]> => {
  try {
    const url = new UrlQuery().UrlGeoLocationNameCity(city);
    const response = await FetchGet(url);
    if (response.statusCode !== 200) {
      return [];
    }
    const cityDataResult = response.data as IGeoLocationNameCity[];
    const result: DropdownResult[] = [];
    for (const cityData of cityDataResult) {
      result.push({
        id: `${cityData.latitude},${cityData.longitude}`,
        displayName: cityData.name,
        altText: getAltText(cityData)
      });
    }
    return result;
  } catch {
    return [];
  }
};

export function getAltText(cityData: IGeoLocationNameCity): string {
  let altText = "";
  if (cityData.province) {
    altText += cityData.province;
  }
  if (cityData.countryName) {
    altText += (altText ? ", " : "") + cityData.countryName;
  }
  return altText;
}

interface IGeoLocationNameCity {
  name: string;
  latitude: number;
  longitude: number;
  province?: string;
  countryName?: string;
}
