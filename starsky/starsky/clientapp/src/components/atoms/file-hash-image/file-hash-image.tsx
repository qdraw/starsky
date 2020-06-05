import React, { useEffect } from 'react';
import { Orientation } from '../../../interfaces/IFileIndexItem';
import FetchGet from '../../../shared/fetch-get';
import { OrientationHelper } from '../../../shared/orientation-helper';
import { UrlQuery } from '../../../shared/url-query';

export interface IFileHashImageProps {
  setError?: React.Dispatch<React.SetStateAction<boolean>>,
  isError: boolean,
  setIsLoading?: React.Dispatch<React.SetStateAction<boolean>>,
  fileHash: string,
  orientation?: Orientation,
  tags?: string,
}

const FileHashImage: React.FunctionComponent<IFileHashImageProps> = (props) => {

  const [isAutomaticRotated, setAutomaticRotated] = React.useState(true);
  useEffect(() => {
    console.log('--eff');
    new OrientationHelper().DetectAutomaticRotation().then((result) => setAutomaticRotated(result))
  }, []);

  // To Get the rotation update
  const [translateRotation, setTranslateRotation] = React.useState(Orientation.Horizontal);
  useEffect(() => {
    if (!props.orientation) return;

    console.log('isAutomaticRotated', isAutomaticRotated);

    if (isAutomaticRotated) {
      return;
    }
    // know if the thumbnail is ready, if not rotate the image clientside
    FetchGet(new UrlQuery().UrlThumbnailJsonApi(props.fileHash)).then((result) => {
      if (!props.orientation) return;
      if (result.statusCode === 202) {
        // result from API is: "Thumbnail is not ready yet"
        setTranslateRotation(props.orientation);
      }
      else if (result.statusCode === 200) {
        // thumbnail is alreay rotated (but need to be called due change of image)
        setTranslateRotation(Orientation.Horizontal);
      }
    }).catch((e) => {
      console.log(e);
    });

  }, [props.fileHash, props.orientation, isAutomaticRotated]);

  return <><img alt={props.tags}
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
    }} src={new UrlQuery().UrlThumbnailImage(props.fileHash, true)} /></>
};

export default FileHashImage;