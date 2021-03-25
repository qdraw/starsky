import { memo } from "react";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";

interface IDetailViewInfoMakeModelApertureProps {
  fileIndexItem: IFileIndexItem;
}

const DetailViewInfoMakeModelAperture: React.FunctionComponent<IDetailViewInfoMakeModelApertureProps> = memo(
  ({ fileIndexItem }) => {
    function ShowISOIfExistCompontent(fileIndexItemInside: IFileIndexItem) {
      return (
        <>
          {fileIndexItemInside.isoSpeed !== 0 ? (
            <>ISO {fileIndexItemInside.isoSpeed}</>
          ) : null}
        </>
      );
    }

    return (
      <>
        {fileIndexItem.make &&
        fileIndexItem.model &&
        fileIndexItem.aperture &&
        fileIndexItem.focalLength ? (
          <div className="box">
            <div className="icon icon--shutter-speed" />
            <b>
              <span data-test="make">{fileIndexItem.make}&nbsp;</span>
              <span data-test="model">{fileIndexItem.model}</span>
            </b>
            <p>
              f/<span data-test="aperture">{fileIndexItem.aperture}</span>
              &nbsp;&nbsp;&nbsp;
              {fileIndexItem.shutterSpeed} sec&nbsp;&nbsp;&nbsp;
              <span data-test="focalLength">
                {fileIndexItem.focalLength.toFixed(1)}
              </span>{" "}
              mm&nbsp;&nbsp;&nbsp;
              <ShowISOIfExistCompontent {...fileIndexItem} />
            </p>
          </div>
        ) : null}
      </>
    );
  }
);

export default DetailViewInfoMakeModelAperture;
