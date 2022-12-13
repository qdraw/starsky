import React, { memo } from "react";
import { DetailViewAction } from "../../../contexts/detailview-context";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useLocation from "../../../hooks/use-location";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import { Language } from "../../../shared/language";
import ModalGeo from "../modal-geo/modal-geo";

interface IDetailViewInfoLocationProps {
  fileIndexItem: IFileIndexItem;
  isFormEnabled: boolean;
  dispatch: React.Dispatch<DetailViewAction> | null;
  setFileIndexItem: React.Dispatch<React.SetStateAction<IFileIndexItem>> | null;
}

const DetailViewInfoLocation: React.FunctionComponent<IDetailViewInfoLocationProps> =
  memo(({ fileIndexItem, isFormEnabled, dispatch, setFileIndexItem }) => {
    const history = useLocation();

    const settings = useGlobalSettings();
    const language = new Language(settings.language);
    const MessageNounNameless = language.text("Naamloze", "Unnamed");
    const MessageNounNone = language.text("Geen enkele", "Not any");

    const MessageLocation = language.text("locatie", "location");

    const [isLocationOpen, setLocationOpen] = React.useState(
      history.location.href.includes("&modal=geo")
    );

    return (
      <>
        {/* loation when the image is created */}
        {isLocationOpen ? (
          <ModalGeo
            latitude={fileIndexItem.latitude}
            longitude={fileIndexItem.longitude}
            parentDirectory={fileIndexItem.parentDirectory}
            selectedSubPath={fileIndexItem.fileName}
            isFormEnabled={isFormEnabled}
            handleExit={(model) => {
              setLocationOpen(false);
              history.navigate(
                history.location.href.replace(/&modal=geo/gi, ""),
                {
                  replace: true
                }
              );
              if (!model || !setFileIndexItem || !dispatch) {
                return;
              }
              setFileIndexItem({
                ...fileIndexItem,
                ...model
              });
              dispatch({
                type: "update",
                ...model
              });
              fileIndexItem.locationCity = model.locationCity;
              fileIndexItem.locationCountry = model.locationCountry;
            }}
            isOpen={true}
          />
        ) : null}

        <a
          className="box"
          onClick={(event) => {
            event.preventDefault();
            history.navigate(
              history.location.href.replace(/&modal=geo/gi, "") + "&modal=geo",
              {
                replace: true
              }
            );
            setLocationOpen(true);
          }}
          href={history.location.href + "&modal=geo"}
        >
          <div
            className="icon icon--location"
            data-test="detailview-location-div"
          />
          {true ? <div className="icon icon--right icon--edit" /> : null}
          {fileIndexItem.locationCity && fileIndexItem.locationCountry ? (
            <>
              <b>{fileIndexItem.locationCity}</b>
              <p>{fileIndexItem.locationCountry}</p>
            </>
          ) : (
            <>
              <b>
                {fileIndexItem.longitude && fileIndexItem.latitude
                  ? MessageNounNameless
                  : MessageNounNone}
              </b>
              <p>{MessageLocation}</p>
            </>
          )}
        </a>
      </>
    );
  });

export default DetailViewInfoLocation;
