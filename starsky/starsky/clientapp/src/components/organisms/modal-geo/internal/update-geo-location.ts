import { IGeoLocationModel } from "../../../../interfaces/IGeoLocationModel";
import { AsciiNull } from "../../../../shared/ascii-null";
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
      model = reverseGeoCodeResult.data as IGeoLocationModel;
      bodyParams.append("locationCity", model.locationCity ?? AsciiNull());
      bodyParams.append("locationCountry", model.locationCountry ?? AsciiNull());
      bodyParams.append("locationCountryCode", model.locationCountryCode ?? AsciiNull());
      bodyParams.append("locationState", model.locationState ?? AsciiNull());
    }
  } catch {
    // do nothing
  }

  try {
    const bodyParamsString = bodyParams.toString().replace(/%00/gi, AsciiNull());
    const updateResult = await FetchPost(new UrlQuery().UrlUpdateApi(), bodyParamsString);
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
