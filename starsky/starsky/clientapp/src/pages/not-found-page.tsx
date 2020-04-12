import { Link, RouteComponentProps } from '@reach/router';
import React, { FunctionComponent } from 'react';
import MenuDefault from '../components/menu-default';
import useGlobalSettings from '../hooks/use-global-settings';
import { Language } from '../shared/language';
import { UrlQuery } from '../shared/url-query';

const NotFoundPage: FunctionComponent<RouteComponentProps<any>> = () => {

  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);

  const MessageNotFound = language.text("Oeps niet gevonden", "Not Found")
  const MessageGoToHome = language.text("Ga naar de homepagina", "Go to the homepage")

  return (<div>
    <MenuDefault isEnabled={true}></MenuDefault>
    <div className="content">
      <div className="content--header"><Link to={new UrlQuery().UrlHomePage()}>{MessageNotFound}</Link></div>
      <div className="content--subheader"><Link to={new UrlQuery().UrlHomePage()}><u>{MessageGoToHome}</u></Link></div>
    </div>
  </div>)
}

export default NotFoundPage;
