import { memo } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import { Language } from "../../../shared/language";

interface IDetailViewExifStatusProps {
  fileIndexItem: IFileIndexItem;
}

const DetailViewExifStatus: React.FunctionComponent<IDetailViewExifStatusProps> = memo(
  ({ fileIndexItem }) => {
    const settings = useGlobalSettings();
    const language = new Language(settings.language);

    const MessageReadOnlyFile = language.text(
      "Alleen lezen bestand",
      "Read only file"
    );
    const MessageNotFoundSourceMissing = language.text(
      "Mist in de index",
      "Misses in the index"
    );
    const MessageServerError = language.text(
      "Er is iets mis met de input",
      "Something is wrong with the input"
    );
    const MessageDeleted = language.text(
      "Staat in de prullenmand",
      "Is in the trash"
    );
    const MessageDeletedRestoreInstruction = language.text(
      "'Zet terug uit prullenmand' om het item te bewerken",
      "'Restore from Trash' to edit the item"
    );

    return (
      <>
        {fileIndexItem.status === IExifStatus.Deleted ||
        fileIndexItem.status === IExifStatus.ReadOnly ||
        fileIndexItem.status === IExifStatus.NotFoundSourceMissing ||
        fileIndexItem.status === IExifStatus.ServerError ? (
          <>
            <div className="content--header">Status</div>
            <div className="content content--text">
              {fileIndexItem.status === IExifStatus.Deleted ? (
                <>
                  <div className="warning-box">{MessageDeleted}</div>
                  {MessageDeletedRestoreInstruction}
                </>
              ) : null}
              {fileIndexItem.status === IExifStatus.NotFoundSourceMissing ? (
                <>
                  <div className="warning-box">
                    {MessageNotFoundSourceMissing}
                  </div>{" "}
                </>
              ) : null}
              {fileIndexItem.status === IExifStatus.ReadOnly ? (
                <>
                  <div className="warning-box">{MessageReadOnlyFile}</div>{" "}
                </>
              ) : null}
              {fileIndexItem.status === IExifStatus.ServerError ? (
                <>
                  <div className="warning-box">{MessageServerError}</div>{" "}
                </>
              ) : null}
            </div>
          </>
        ) : null}
      </>
    );
  }
);
export default DetailViewExifStatus;
