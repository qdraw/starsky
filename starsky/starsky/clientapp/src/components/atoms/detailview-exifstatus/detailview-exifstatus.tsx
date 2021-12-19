import { memo } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import { Language } from "../../../shared/language";

interface IDetailViewExifStatusProps {
  status: IExifStatus;
}

const DetailViewExifStatus: React.FunctionComponent<IDetailViewExifStatusProps> =
  memo(({ status }) => {
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

    const DeleteComponent = (
      <>
        {status === IExifStatus.Deleted ? (
          <>
            <div
              data-test="detailview-exifstatus-status-deleted"
              className="warning-box"
            >
              {MessageDeleted}
            </div>
            {MessageDeletedRestoreInstruction}
          </>
        ) : null}
      </>
    );
    const NotFoundSourceMissingComponent = (
      <>
        {status === IExifStatus.NotFoundSourceMissing ? (
          <>
            <div
              data-test="detailview-exifstatus-status-source-missing"
              className="warning-box"
            >
              {MessageNotFoundSourceMissing}
            </div>{" "}
          </>
        ) : null}
      </>
    );
    const ReadOnlyFileComponent = (
      <>
        {status === IExifStatus.ReadOnly ? (
          <>
            <div
              data-test="detailview-exifstatus-status-read-only"
              className="warning-box"
            >
              {MessageReadOnlyFile}
            </div>{" "}
          </>
        ) : null}
      </>
    );

    const ServerErrorComponent = (
      <>
        {status === IExifStatus.ServerError ? (
          <>
            <div
              data-test="detailview-exifstatus-status-server-error"
              className="warning-box"
            >
              {MessageServerError}
            </div>{" "}
          </>
        ) : null}
      </>
    );

    return (
      <>
        {status === IExifStatus.Deleted ||
        status === IExifStatus.ReadOnly ||
        status === IExifStatus.NotFoundSourceMissing ||
        status === IExifStatus.ServerError ? (
          <>
            <div className="content--header">Status</div>
            <div className="content content--text">
              {DeleteComponent}
              {NotFoundSourceMissingComponent}
              {ReadOnlyFileComponent}
              {ServerErrorComponent}
            </div>
          </>
        ) : null}
      </>
    );
  });

export default DetailViewExifStatus;
