import { Link } from '@reach/router';
import React, { memo } from "react";
import useGlobalSettings from '../hooks/use-globalSettings';
import useLocation from '../hooks/use-location';
import { IRelativeObjects } from "../interfaces/IDetailView";
import { Language } from '../shared/language';
import { URLPath } from '../shared/url-path';

export interface IRelativeLink {
  relativeObjects: IRelativeObjects;
}

const RelativeLink: React.FunctionComponent<IRelativeLink> = memo((props) => {

  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessagePrevious = language.text("Vorige", "Previous");
  const MessageNext = language.text("Volgende", "Next");

  // used for reading current location
  var history = useLocation();

  let { relativeObjects } = props;

  if (!relativeObjects) return (<div className="relativelink" />)

  var prevUrl = new URLPath().updateFilePath(history.location.search, relativeObjects.prevFilePath);
  var nextUrl = new URLPath().updateFilePath(history.location.search, relativeObjects.nextFilePath);

  let prev = relativeObjects.prevFilePath !== null ?
    <Link className="prev" to={prevUrl}>{MessagePrevious}</Link> : null;
  let next = relativeObjects.nextFilePath !== null ?
    <Link className="next" to={nextUrl}>{MessageNext}</Link> : null;

  return (<div className="relativelink"><h4 className="nextprev">
    {prev}
    {next}
  </h4></div>);
});
export default RelativeLink
