import { Link } from '@reach/router';
import React, { memo } from "react";
import useGlobalSettings from '../../../hooks/use-global-settings';
import useLocation from '../../../hooks/use-location';
import { IRelativeObjects } from "../../../interfaces/IDetailView";
import { Language } from '../../../shared/language';
import { UrlQuery } from '../../../shared/url-query';

export interface IRelativeLink {
  relativeObjects: IRelativeObjects;
}

/**
 * Only for Archive pages (used to be RelativeLink)
 */
const ArchivePagination: React.FunctionComponent<IRelativeLink> = memo((props) => {

  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessagePrevious = language.text("Vorige", "Previous");
  const MessageNext = language.text("Volgende", "Next");

  // used for reading current location
  var history = useLocation();

  let { relativeObjects } = props;

  if (!relativeObjects) return (<div className="relativelink" />)

  // to the next/prev relative object
  var prevUrl = new UrlQuery().updateFilePathHash(history.location.search, relativeObjects.prevFilePath);
  var nextUrl = new UrlQuery().updateFilePathHash(history.location.search, relativeObjects.nextFilePath);

  let prev = relativeObjects.prevFilePath !== null ?
    <Link className="prev" to={prevUrl}>{MessagePrevious}</Link> : null;
  let next = relativeObjects.nextFilePath !== null ?
    <Link className="next" to={nextUrl}>{MessageNext}</Link> : null;

  return (<div className="relativelink"><h4 className="nextprev">
    {prev}
    {next}
  </h4></div>);
});
export default ArchivePagination
