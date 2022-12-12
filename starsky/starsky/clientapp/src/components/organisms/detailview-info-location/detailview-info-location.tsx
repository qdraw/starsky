import React, { memo } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useLocation from "../../../hooks/use-location";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import { Language } from "../../../shared/language";
import ModalGeo from "../modal-geo/modal-geo";

interface IDetailViewInfoLocationProps {
  fileIndexItem: IFileIndexItem;
}

const DetailViewInfoLocation: React.FunctionComponent<IDetailViewInfoLocationProps> =
  memo(({ fileIndexItem }) => {
    const history = useLocation();

    const settings = useGlobalSettings();
    const language = new Language(settings.language);
    const MessageNounNameless = language.text("Naamloze", "Unnamed");
    const MessageNounNone = language.text("Geen enkele", "Not any");

    const MessageLocation = language.text("locatie", "location");

    const [isLocationOpen, setLocationOpen] = React.useState(
      history.location.search.includes("&modal=geo")
    );

    return (
      <>
        {/* loation when the image is created */}
        {isLocationOpen ? (
          <ModalGeo
            latitude={fileIndexItem.latitude}
            longitude={fileIndexItem.latitude}
            parentDirectory={"/test"}
            selectedSubPath={"/test"}
            // subPath={fileIndexItem.filePath}
            handleExit={() => {
              setLocationOpen(false);
              // if (!result || !result[0]) return;
              // // only update the content that can be changed
              // setFileIndexItem({
              //   ...fileIndexItem,
              //   dateTime: result[0].dateTime
              // });
              // dispatch({
              //   type: "update",
              //   dateTime: result[0].dateTime,
              //   lastEdited: ""
              // });
            }}
            isOpen={true}
          />
        ) : null}

        <a
          className="box"
          onClick={(event) => {
            event.preventDefault();
            setLocationOpen(true);
          }}
          href={"&modal=geo"}
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

        {/* {fileIndexItem.latitude && fileIndexItem.longitude ? (
          <a
            className="box"
            target="_blank"
            rel="noopener noreferrer"
            href={
              "https://www.openstreetmap.org/?mlat=" +
              fileIndexItem.latitude +
              "&mlon=" +
              fileIndexItem.longitude +
              "#map=16/" +
              fileIndexItem.latitude +
              "/" +
              fileIndexItem.longitude
            }
          >
            <div
              className="icon icon--location"
              data-test="detailview-location-div"
            />
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
                    : MessageNounNone}{" "}
                  {fileIndexItem.longitude && fileIndexItem.latitude}
                </b>
                <p>{MessageLocation}</p>
              </>
            )}
          </a>
        ) : (
          ""
        )} */}
      </>
    );
  });

export default DetailViewInfoLocation;
