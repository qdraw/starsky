import { FunctionComponent } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import localization from "../../../localization/localization.json";
import { Language } from "../../../shared/language";
import MenuDefault from "../menu-default/menu-default";

const ApplicationException: FunctionComponent = () => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageApplicationException = language.key(localization.MessageApplicationException);
  const MessageRefreshPageTryAgain = language.key(localization.MessageRefreshPageTryAgain);

  return (
    <>
      <MenuDefault isEnabled={false} />
      <div className="content--header fade-in" data-test="application-exception-header">
        {MessageApplicationException}
      </div>
      <div className="content--subheader fade-in">
        <button
          data-test="reload"
          className="btn btn--default"
          onClick={() => window.location.reload()}
        >
          {MessageRefreshPageTryAgain}
        </button>
      </div>
    </>
  );
};

export default ApplicationException;
