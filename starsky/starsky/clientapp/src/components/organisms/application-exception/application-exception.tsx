import React, { FunctionComponent } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { Language } from "../../../shared/language";
import MenuDefault from "../menu-default/menu-default";

const ApplicationException: FunctionComponent<any> = () => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageApplicationException = language.text(
    "We hebben een op dit moment een verstoring op de applicatie",
    "We have a disruption on the application right now"
  );
  const MessageRefreshPageTryAgain = language.text(
    "Herlaad de applicatie om het opnieuw te proberen",
    "Please reload the application to try again"
  );

  return (
    <>
      <MenuDefault isEnabled={false} />
      <div className="content--header" data-test="application-exception-header">
        {MessageApplicationException}
      </div>
      <div className="content--subheader">
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
