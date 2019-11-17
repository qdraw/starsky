import { Link } from '@reach/router';
import React, { memo } from "react";
import useLocation from '../hooks/use-location';
import { IRelativeObjects } from "../interfaces/IDetailView";
import { URLPath } from '../shared/url-path';

export interface IRelativeLink {
  relativeObjects: IRelativeObjects;
}

const RelativeLink: React.FunctionComponent<IRelativeLink> = memo((props) => {

  // used for reading current location
  var history = useLocation();

  let { relativeObjects } = props;

  if (!relativeObjects || !relativeObjects.prevFilePath || !relativeObjects.nextFilePath) return (<div className="relativelink"></div>)

  var prevUrl = new URLPath().updateFilePath(history.location.search, relativeObjects.prevFilePath);
  var nextUrl = new URLPath().updateFilePath(history.location.search, relativeObjects.nextFilePath);

  let prev = relativeObjects.prevFilePath !== null ?
    <Link className="prev" to={prevUrl}>Vorige</Link> : null;
  let next = relativeObjects.nextFilePath !== null ?
    <Link className="next" to={nextUrl}>Volgende</Link> : null;

  return (<div className="relativelink"><h4 className="nextprev">
    {prev}
    {next}
  </h4></div>);


});
export default RelativeLink