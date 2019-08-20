import React, { memo, useContext } from "react";
import HistoryContext from '../contexts/history-contexts';
import { IRelativeObjects } from "../interfaces/IDetailView";
import { URLPath } from '../shared/url-path';

export interface IRelativeLink {
  relativeObjects: IRelativeObjects;
}

const RelativeLink: React.FunctionComponent<IRelativeLink> = memo((props) => {

  // used for reading current location
  const history = useContext(HistoryContext);

  let { relativeObjects } = props;
  if (!relativeObjects) return (<div className="relativelink" />);

  var prevUrl = new URLPath().updateFilePath(history.location.hash, relativeObjects.prevFilePath);
  var nextUrl = new URLPath().updateFilePath(history.location.hash, relativeObjects.nextFilePath);
  let prev = relativeObjects.prevFilePath === null ? "" : <a className="prev" href={prevUrl}>Vorige</a>;
  let next = relativeObjects.nextFilePath === null ? "" : <a className="next" href={nextUrl}>Volgende</a>;

  return (<div className="relativelink"><h4 className="nextprev">
    {prev}
    {next}
  </h4></div>);


});
export default RelativeLink