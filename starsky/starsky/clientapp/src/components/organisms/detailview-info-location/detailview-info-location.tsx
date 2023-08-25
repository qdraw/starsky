import React, { memo } from "react";
import { DetailViewAction } from "../../../contexts/detailview-context";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useLocation from "../../../hooks/use-location/use-location";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import { IGeoLocationModel } from "../../../interfaces/IGeoLocationModel";
import { Language } from "../../../shared/language";
import ModalGeo from "../modal-geo/modal-geo";

interface IDetailViewInfoLocationProps {
  locationCountry?: string;
  locationCity?: string;
  isFormEnabled: boolean;
  latitude?: number;
  longitude?: number;
  fileIndexItem: IFileIndexItem;
  dispatch: React.Dispatch<DetailViewAction> | null;
  setFileIndexItem: React.Dispatch<React.SetStateAction<IFileIndexItem>> | null;
}

const DetailViewInfoLocation: React.FunctionComponent<IDetailViewInfoLocationProps> =
  memo(
    ({
      latitude,
      longitude,
      locationCountry,
      locationCity,
      isFormEnabled,
      dispatch,
      fileIndexItem,
      setFileIndexItem
    }) => {
      const history = useLocation();

      const settings = useGlobalSettings();
      const language = new Language(settings.language);
      const MessageNounNameless = language.text("Naamloze", "Unnamed");
      const MessageNounNone = language.text("Geen enkele", "Not any");

      const MessageLocation = language.text("locatie", "location");

      const [isLocationOpen, setLocationOpen] = React.useState(
        history.location.href?.includes("&modal=geo")
      );

      function handleExit(model: IGeoLocationModel | null) {
        setLocationOpen(false);
        history.navigate(history.location.href.replace(/&modal=geo/gi, ""), {
          replace: true
        });

        // when no data is passed, but window should be closed
        if (!model || !setFileIndexItem || !dispatch) {
          return;
        }
        setFileIndexItem({
          ...fileIndexItem,
          ...model
        });
        dispatch({
          type: "update",
          ...model,
          filePath: fileIndexItem.filePath
        });
        locationCity = model.locationCity;
        locationCountry = model.locationCountry;
      }

      function onClick(event: React.MouseEvent<HTMLAnchorElement, MouseEvent>) {
        event.preventDefault();
        history.navigate(
          history.location.href.replace(/&modal=geo/gi, "") + "&modal=geo",
          {
            replace: true
          }
        );
        setLocationOpen(true);
      }

      return (
        <>
          {/* loation when the image is created */}
          {isLocationOpen ? (
            <ModalGeo
              latitude={latitude}
              longitude={longitude}
              parentDirectory={fileIndexItem.parentDirectory}
              selectedSubPath={fileIndexItem.fileName}
              isFormEnabled={isFormEnabled}
              handleExit={handleExit}
              isOpen={true}
            />
          ) : null}

          <a
            className="box"
            onClick={onClick}
            data-test="detailview-info-location-open-modal"
            href={history.location.href + "&modal=geo"}
          >
            <div
              className="icon icon--location"
              data-test="detailview-location-div"
            />
            <div className="icon icon--right icon--edit" />
            {locationCity && locationCountry ? (
              <>
                <b data-test="detailview-info-location-city">{locationCity}</b>
                <p>{locationCountry}</p>
              </>
            ) : (
              <>
                <b>
                  {longitude && latitude
                    ? MessageNounNameless
                    : MessageNounNone}
                </b>
                <p>{MessageLocation}</p>
              </>
            )}
          </a>
        </>
      );
    }
  );

export default DetailViewInfoLocation;
