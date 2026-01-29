import FetchGet from "../../../../shared/fetch/fetch-get";
import { UrlQuery } from "../../../../shared/url/url-query";
import { DropdownResult } from "../../../atoms/searchable-dropdown/ISearableDropdownProps";

export const fetchCityTimezones = async (
  dateTime: string,
  city: string
): Promise<DropdownResult[]> => {
  const url = new UrlQuery().UrlGeoLocationNameCityTimezone(dateTime, city);

  const response = await FetchGet(url);
  if (response.statusCode !== 200) {
    return [];
  }

  return response.data as DropdownResult[];
};
