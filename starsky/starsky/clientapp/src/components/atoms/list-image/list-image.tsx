import React, { memo, useEffect, useRef, useState } from "react";
import EmptyImage from "../../../../src/style/images/empty-image.gif";
import useIntersection from "../../../hooks/use-intersection-observer";
import useLocation from "../../../hooks/use-location/use-location";
import { ImageFormat } from "../../../interfaces/IFileIndexItem";
import { URLPath } from "../../../shared/url-path";
import { UrlQuery } from "../../../shared/url-query";

interface IListImageProps {
  children?: React.ReactNode;
  fileHash: string;
  imageFormat?: ImageFormat;
  alt?: string;
}

/**
 * Used inside archive/search
 */
const ListImage: React.FunctionComponent<IListImageProps> = memo((props) => {
  const target = useRef<HTMLDivElement>(null);
  const alt = props.alt ? props.alt : "afbeelding";

  const [src, setSrc] = useState(props.fileHash);

  const [alwaysLoadImage] = useState(
    localStorage.getItem("alwaysLoadImage") === "true"
  );

  // Reset Loading after changing page
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(false);

  // to update the url, and trigger loading if the url is changed
  useEffect(() => {
    setError(false);
    setIsLoading(true);
  }, [props.fileHash]);

  const intersected = useIntersection(target, {
    rootMargin: "250px",
    once: true,
    threshold: 0.3
  });
  // threshold = indicate at what percentage of the target's visibility the callback is executed. (default = 0)

  // to stop loading images after a url change
  const history = useLocation();
  const [historyLocation] = useState(history.location.search);
  useEffect(() => {
    // use ?f only to support details
    // need to refresh
    if (
      new URLPath().getFilePath(historyLocation) !==
        new URLPath().getFilePath(history.location.search) &&
      isLoading
    ) {
      // data:images are blocked by a strict CSP
      setSrc(EmptyImage); // 26 bytes
      return;
    }
    setSrc(new UrlQuery().UrlThumbnailImage(props.fileHash, alwaysLoadImage));
  }, [
    props.fileHash,
    history.location.search,
    historyLocation,
    isLoading,
    alwaysLoadImage
  ]);

  if (
    props.fileHash === "null" ||
    props.fileHash === null ||
    !props.fileHash ||
    !props.imageFormat
  ) {
    return (
      <div
        ref={target}
        data-test="list-image-img-error"
        className="img-box--error"
      />
    );
  }

  // for example show gpx, raw and mp4 as icon
  if (
    props.imageFormat !== ImageFormat.bmp &&
    props.imageFormat !== ImageFormat.gif &&
    props.imageFormat !== ImageFormat.jpg &&
    props.imageFormat !== ImageFormat.png
  ) {
    return (
      <div
        ref={target}
        data-test="list-image-img-error"
        className={`img-box--error img-box--unsupported img-box--${props.imageFormat}`}
      />
    );
  }

  let className = "img-box";
  if (error) {
    className += ` img-box--error img-box--${props.imageFormat}`;
  } else if (isLoading) {
    className += " img-box--loading";
  }

  return (
    <div
      ref={target}
      className={className}
      data-test="list-image-img-parent-div"
    >
      {intersected ? (
        <img
          src={src}
          alt={alt}
          data-test="list-image-img"
          onLoad={() => {
            setError(false);
            setIsLoading(false);
          }}
          onError={() => setError(true)}
        />
      ) : (
        <div className="img-box--loading" />
      )}
    </div>
  );
});

export default ListImage;
