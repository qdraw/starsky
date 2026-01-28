import { ITimezone } from "../../../../interfaces/ITimezone";
import FetchGet from "../../../../shared/fetch/fetch-get";
import { UrlQuery } from "../../../../shared/url/url-query";
import { ITimezoneState } from "../hooks/use-timezone-state";

// Africa/Ouagadougou is UTC+0 same as Europe/London
export const defaultRecordedTimezoneId = "Africa/Ouagadougou";
// Do the same because then you don't have to init load
export const defaultCorrectTimezoneId = "Africa/Ouagadougou";

export async function loadTimezones(
  timezoneState: ITimezoneState,
  setError: React.Dispatch<React.SetStateAction<string | null>>,
  dateTime: string
) {
  try {
    const response = await FetchGet(new UrlQuery().UrlTimezones(dateTime));
    if (response.statusCode === 200 && Array.isArray(response.data)) {
      timezoneState.setTimezones(response.data);
      // Set default values
      if (response.data.length > 0) {
        const defaultRecordedTimezone = response.data.find(
          (tz: ITimezone) => tz.id === defaultRecordedTimezoneId
        );
        const defaultCorrectTimezone = response.data.find(
          (tz: ITimezone) => tz.id === defaultCorrectTimezoneId
        );
        timezoneState.setRecordedTimezone(defaultRecordedTimezone?.id || response.data[0].id);
        timezoneState.setCorrectTimezone(defaultCorrectTimezone?.id || response.data[0].id);
      }
    }
  } catch (err) {
    console.error("Failed to load timezones", err);
    setError("Failed to load timezones");
  }
}
