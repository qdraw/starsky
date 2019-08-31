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
  if (!relativeObjects) return (<div className="relativelink" />);

  var prevUrl = new URLPath().updateFilePath(history.location.search, relativeObjects.prevFilePath);
  var nextUrl = new URLPath().updateFilePath(history.location.search, relativeObjects.nextFilePath);
  let prev = relativeObjects.prevFilePath === null ? "" : <a className="prev" href={prevUrl}>Vorige</a>;
  let next = relativeObjects.nextFilePath === null ? "" : <a className="next" href={nextUrl}>Volgende</a>;

  return (<div className="relativelink"><h4 className="nextprev">
    {prev}
    {next}
  </h4></div>);


});
export default RelativeLink