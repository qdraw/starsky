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
}

const FileHashImage: React.FunctionComponent<IFileHashImageProps> = (props) => {
  // To Get the rotation update
  const [translateRotation, setTranslateRotation] = React.useState(
    Orientation.Horizontal
  );
  useEffect(() => {
    (async () => {
      if (!props.orientation) return;
      var isAutomaticRotated = await DetectAutomaticRotation();

      if (isAutomaticRotated) {
        return;
      }
      var result = await FetchGet(
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
    new UrlQuery().UrlThumbnailImage(props.fileHash, true)
  );

  useEffect(() => {
    setImageUrl(new UrlQuery().UrlThumbnailImage(props.fileHash, true));
  }, [props.fileHash]);

  return (
    <PanAndZoomImage
      setError={props.setError}
      setIsLoading={props.setIsLoading}
      translateRotation={translateRotation}
      onWheelCallback={() => {
        setImageUrl(new UrlQuery().UrlThumbnailZoom(props.fileHash, 1));
      }}
      src={imageUrl}
    />
  );
};

export default FileHashImage;
