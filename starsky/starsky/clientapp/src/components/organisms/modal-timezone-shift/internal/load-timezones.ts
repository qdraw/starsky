import FetchGet from "../../../../shared/fetch/fetch-get";
import { UrlQuery } from "../../../../shared/url/url-query";

export async function loadTimezones() {
  try {
    const response = await FetchGet(new UrlQuery().UrlTimezones());
    if (response.statusCode === 200 && Array.isArray(response.data)) {
      setTimezones(response.data);
      // Set default values
      if (response.data.length > 0) {
        const europeLondon = response.data.find((tz: ITimezone) => tz.id === "Europe/London");
        const europeAmsterdam = response.data.find((tz: ITimezone) => tz.id === "Europe/Amsterdam");
        setRecordedTimezone(europeLondon?.id || response.data[0].id);
        setCorrectTimezone(europeAmsterdam?.id || response.data[0].id);
      }
    }
  } catch (err) {
    console.error("Failed to load timezones", err);
    setError("Failed to load timezones");
  }
}
