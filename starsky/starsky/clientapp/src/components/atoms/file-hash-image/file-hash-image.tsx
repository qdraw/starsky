import React, { useEffect } from "react";
import useKeyboardEvent from "../../../hooks/use-keyboard-event";
import { Orientation } from "../../../interfaces/IFileIndexItem";
import DetectAutomaticRotation from "../../../shared/detect-automatic-rotation";
import FetchGet from "../../../shared/fetch-get";
import { Keyboard } from "../../../shared/keyboard";
import { UrlQuery } from "../../../shared/url-query";

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

  // short key to toggle sidemenu
  useKeyboardEvent(
    /^(z)$/,
    (event: KeyboardEvent) => {
      if (new Keyboard().isInForm(event)) return;
      console.log("z");
      setImageUrl(new UrlQuery().UrlThumbnailZoom(props.fileHash, 1));
    },
    [props.fileHash]
  );

  return (
    <>
      <img
        alt={props.tags}
        className={"image--default " + translateRotation}
        onLoad={() => {
          if (!props.setError || !props.setIsLoading) {
            return;
          }
          props.setError(false);
          props.setIsLoading(false);
        }}
        onError={() => {
          if (!props.setError || !props.setIsLoading) {
            return;
          }
          props.setError(true);
          props.setIsLoading(false);
        }}
        src={imageUrl}
      />
    </>
  );
};

export default FileHashImage;
