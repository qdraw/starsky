import { FunctionComponent } from "react";
import Link from "../components/atoms/link/link";
import MenuDefault from "../components/organisms/menu-default/menu-default";
import useGlobalSettings from "../hooks/use-global-settings";
import localization from "../localization/localization.json";
import { Language } from "../shared/language";
import { UrlQuery } from "../shared/url-query";

export const NotFoundPage: FunctionComponent = () => {
  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);

  const MessageNotFound = language.key(localization.MessageNotFound);
  const MessageGoToHome = language.key(localization.MessageGoToHome);

  return (
    <div>
      <MenuDefault isEnabled={true}></MenuDefault>
      <div data-test="not-found-page" className="content">
        <div className="content--header">
          <Link to={new UrlQuery().UrlHomePage()}>{MessageNotFound}</Link>
        </div>
        <div className="content--subheader">
          <Link to={new UrlQuery().UrlHomePage()}>
            <u>{MessageGoToHome}</u>
          </Link>
        </div>
      </div>
    </div>
  );
};
