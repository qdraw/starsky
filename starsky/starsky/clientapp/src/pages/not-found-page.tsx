import { FunctionComponent } from "react";
import Link from "../components/atoms/link/link";
import MenuDefault from "../components/organisms/menu-default/menu-default";
import useGlobalSettings from "../hooks/use-global-settings";
import { Language } from "../shared/language";
import { UrlQuery } from "../shared/url-query";

const NotFoundPage: FunctionComponent = () => {
  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);

  const MessageNotFound = language.text("Oeps niet gevonden", "Not Found");
  const MessageGoToHome = language.text(
    "Ga naar de homepagina",
    "Go to the homepage"
  );

  return (
    <div>
      <MenuDefault isEnabled={true}></MenuDefault>
      <div className="content">
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

export default NotFoundPage;
