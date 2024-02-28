import React, { memo } from "react";
import { DetailViewAction } from "../../../contexts/detailview-context";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import localization from "../../../localization/localization.json";
import { isValidDate, parseDate, parseTime } from "../../../shared/date";
import { Language } from "../../../shared/language";
import ModalDatetime from "../modal-edit-date-time/modal-edit-datetime";

interface IDetailViewInfoDateTimeProps {
  fileIndexItem: IFileIndexItem;
  isFormEnabled: boolean;
  setFileIndexItem: React.Dispatch<React.SetStateAction<IFileIndexItem>>;
  dispatch: React.Dispatch<DetailViewAction>;
}

const DetailViewInfoDateTime: React.FunctionComponent<IDetailViewInfoDateTimeProps> = memo(
  ({ fileIndexItem, isFormEnabled, setFileIndexItem, dispatch }) => {
    const settings = useGlobalSettings();
    const language = new Language(settings.language);
    const MessageCreationDate = language.key(localization.MessageCreationDate);
    const MessageCreationDateIsAtUnknownTime = language.key(
      localization.MessageCreationDateIsAtUnknownTime
    );

    const [modalDatetimeOpen, setModalDatetimeOpen] = React.useState(false);

    function handleExit(result: IFileIndexItem[] | null) {
      setModalDatetimeOpen(false);
      if (!result?.[0]) return;
      // only update the content that can be changed
      setFileIndexItem({
        ...fileIndexItem,
        dateTime: result[0].dateTime
      });
      dispatch({
        filePath: fileIndexItem.filePath,
        type: "update",
        dateTime: result[0].dateTime,
        lastEdited: ""
      });
    }

    return (
      <>
        {/* dateTime when the image is created */}
        {modalDatetimeOpen ? (
          <ModalDatetime
            subPath={fileIndexItem.filePath}
            dateTime={fileIndexItem.dateTime}
            handleExit={handleExit}
            isOpen={true}
          />
        ) : null}

        <button
          className="box"
          disabled={!isFormEnabled}
          data-test="dateTime"
          onClick={() => setModalDatetimeOpen(true)}
        >
          {isFormEnabled ? <div className="icon icon--right icon--edit" /> : null}
          <div className="icon icon--date" />
          {isValidDate(fileIndexItem.dateTime) ? (
            <>
              <b>{parseDate(fileIndexItem.dateTime, settings.language)}</b>
              <p>{parseTime(fileIndexItem.dateTime)}</p>
            </>
          ) : null}
          {!isValidDate(fileIndexItem.dateTime) ? (
            <>
              <b>{MessageCreationDate}</b>
              <p>{MessageCreationDateIsAtUnknownTime}</p>
            </>
          ) : null}
        </button>
      </>
    );
  }
);

export default DetailViewInfoDateTime;
