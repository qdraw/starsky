import { IGeoLocationModel } from "../../../../interfaces/IGeoLocationModel";
import FetchGet from "../../../../shared/fetch/fetch-get";
import FetchPost from "../../../../shared/fetch/fetch-post";
import { URLPath } from "../../../../shared/url/url-path";
import { UrlQuery } from "../../../../shared/url/url-query";
import { ILatLong } from "../modal-geo";

export async function UpdateGeoLocation(
  parentDirectory: string,
  selectedSubPath: string,
  location: ILatLong | null,
  setError: React.Dispatch<React.SetStateAction<boolean>>,
  setIsLoading: React.Dispatch<React.SetStateAction<boolean>>,
  collections?: boolean
): Promise<IGeoLocationModel | null> {
  if (!location?.latitude || !location?.longitude) {
    return Promise.resolve(null);
  }

  setIsLoading(true);
  const bodyParams = new URLPath().ObjectToSearchParams({
    collections,
    f: parentDirectory + "/" + selectedSubPath,
    append: false
  });
  bodyParams.append("latitude", location.latitude.toString());
  bodyParams.append("longitude", location.longitude.toString());

  let model = {} as IGeoLocationModel;
  try {
    const reverseGeoCodeResult = await FetchGet(
      new UrlQuery().UrlReverseLookup(location.latitude.toString(), location.longitude.toString())
    );
    if (reverseGeoCodeResult.statusCode === 200) {
      model = reverseGeoCodeResult.data;
      bodyParams.append("locationCity", model.locationCity);
      bodyParams.append("locationCountry", model.locationCountry);
      bodyParams.append("locationCountryCode", model.locationCountryCode);
      bodyParams.append("locationState", model.locationState);
    }
    console.log(reverseGeoCodeResult.statusCode);
  } catch {
    // do nothing
  }

  console.log(bodyParams.toString());

  try {
    const updateResult = await FetchPost(new UrlQuery().UrlUpdateApi(), bodyParams.toString());
    if (updateResult.statusCode !== 200) {
      setError(true);
      setIsLoading(false);
      return Promise.resolve(null);
    }
  } catch {
    setError(true);
    setIsLoading(false);
    return Promise.resolve(null);
  }

  setIsLoading(false);
  return Promise.resolve(model);
}
