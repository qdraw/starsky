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
    "Herlaad de pagina om het opnieuw te proberen",
    "Please reload the page to try again"
  );

  return (
    <>
      <MenuDefault isEnabled={false} />
      <div className="content--header">{MessageApplicationException}</div>
      <div className="content--subheader">{MessageRefreshPageTryAgain}</div>
    </>
  );
};

export default ApplicationException;
