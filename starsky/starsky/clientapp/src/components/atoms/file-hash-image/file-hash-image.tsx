import React, { useEffect } from "react";
import { Orientation } from "../../../interfaces/IFileIndexItem";
import DetectAutomaticRotation from "../../../shared/detect-automatic-rotation";
import FetchGet from "../../../shared/fetch-get";
import { UrlQuery } from "../../../shared/url-query";
import PanAndZoomImage from "./pan-and-zoom-image";

export interface IFileHashImageProps {
  setError?: React.Dispatch<React.SetStateAction<boolean>>;
  isError: boolean;
  setIsLoading?: React.Dispatch<React.SetStateAction<boolean>>;
  fileHash: string;
  orientation?: Orientation;
  tags?: string;
  id?: string; // filepath to know when image is changed
  onWheelCallback?(z: number): void;
  onResetCallback?(): void;
  onErrorCallback?(): void;
}

const FileHashImage: React.FunctionComponent<IFileHashImageProps> = (props) => {
  // To Get the rotation update
  const [translateRotation, setTranslateRotation] = React.useState(
    Orientation.Horizontal
  );
  useEffect(() => {
    (async () => {
      if (!props.orientation) return;
      const isAutomaticRotated = await DetectAutomaticRotation();

      if (isAutomaticRotated) {
        return;
      }
			const result = await FetchGet(
        new UrlQuery().UrlThumbnailJsonApi(props.fileHash)
      );

      if (result.statusCode === 202) {
        // result from API is: "Thumbnail is not ready yet"
        setTranslateRotation(props.orientation);
      } else if (result.statusCode === 200) {
        // thumbnail is alreay rotated (but need to be called due change of image)
        setTranslateRotation(Orientation.Horizontal);
      }
    })();
  }, [props.fileHash, props.orientation]);

  const [imageUrl, setImageUrl] = React.useState(
    new UrlQuery().UrlThumbnailImageLargeOrExtraLarge(
      props.fileHash,
      window.innerWidth > 1000
    )
  );

  useEffect(() => {
    setImageUrl(
      new UrlQuery().UrlThumbnailImageLargeOrExtraLarge(
        props.fileHash,
        window.innerWidth > 1000
      )
    );
  }, [props.fileHash]);

  function onWheelCallback(z: number) {
    setImageUrl(new UrlQuery().UrlThumbnailZoom(props.fileHash, props.id, 1));
    if (props.onWheelCallback) props.onWheelCallback(z);
  }

  return (
    <PanAndZoomImage
      id={props.id}
      setError={props.setError}
      onErrorCallback={props.onErrorCallback}
      setIsLoading={props.setIsLoading}
      translateRotation={translateRotation}
      onWheelCallback={onWheelCallback}
      onResetCallback={() => {
        setImageUrl(
          new UrlQuery().UrlThumbnailImageLargeOrExtraLarge(
            props.fileHash,
            window.innerWidth > 1000
          )
        );
        if (props.onResetCallback) props.onResetCallback();
      }}
      src={imageUrl}
    />
  );
};

export default FileHashImage;
