import { memo } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { IExifStatus } from "../../../interfaces/IExifStatus";
import localization from "../../../localization/localization.json";
import { Language } from "../../../shared/language";

interface IDetailViewExifStatusProps {
  status: IExifStatus;
}

const DetailViewExifStatus: React.FunctionComponent<IDetailViewExifStatusProps> =
  memo(({ status }) => {
    const settings = useGlobalSettings();
    const language = new Language(settings.language);

    const MessageReadOnlyFile = language.key(localization.MessageReadOnlyFile);

    const MessageNotFoundSourceMissing = language.key(
      localization.MessageNotFoundSourceMissing
    );
    const MessageServerError = language.key(
      localization.MessageServerInputError
    );
    const MessageIsInTheTrash = language.key(localization.MessageIsInTheTrash);

    const MessageDeletedRestoreInstruction = language.key(
      localization.MessageDeletedRestoreInstruction
    );

    const DeleteComponent = (
      <>
        {status === IExifStatus.Deleted ? (
          <>
            <div
              data-test="detailview-exifstatus-status-deleted"
              className="warning-box"
            >
              {MessageIsInTheTrash}
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
