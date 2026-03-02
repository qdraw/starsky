import FetchGet from "../../../../shared/fetch/fetch-get";
import { UrlQuery } from "../../../../shared/url/url-query";
import { DropdownResult } from "../../../atoms/searchable-dropdown/ISearableDropdownProps";

export const fetchCity = async (city: string): Promise<DropdownResult[]> => {
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
      altText: cityData.name
    });
  }

  console.log(result);
  return result;
};

interface IGeoLocationNameCity {
  name: string;
  latitude: number;
  longitude: number;
}
