import { URLPath } from "../../../../shared/url/url-path";
import { UrlQuery } from "../../../../shared/url/url-query";

export function SetDefaultEffect(
  historyLocationSearch: string,
  setDownloadPhotoApi: React.Dispatch<React.SetStateAction<string>>,
  videoRef: React.RefObject<HTMLVideoElement>,
  scrubberRef: React.RefObject<HTMLSpanElement>,
  progressRef: React.RefObject<HTMLProgressElement>,
  timeRef: React.RefObject<HTMLSpanElement>
) {
  const downloadApiLocal = new UrlQuery().UrlDownloadPhotoApi(
    new URLPath().encodeURI(new URLPath().getFilePath(historyLocationSearch)),
    false,
    true
  );
  setDownloadPhotoApi(downloadApiLocal);

  if (!videoRef.current || !scrubberRef.current || !progressRef.current || !timeRef.current) {
    return;
  }

  videoRef.current.setAttribute("src", downloadApiLocal);
  videoRef.current.load();

  // after a location change
  progressRef.current.removeAttribute("max");
  videoRef.current.currentTime = 0;
  scrubberRef.current.style.left = "0%";
  progressRef.current.value = 0;
  timeRef.current.innerHTML = "";
}
