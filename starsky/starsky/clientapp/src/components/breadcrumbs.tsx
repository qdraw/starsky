import { Link } from '@reach/router';
import React, { memo } from "react";
import { URLPath } from '../shared/url-path';

interface IBreadcrumbProps {
  subPath: string;
  breadcrumb: Array<string>;
}

const Breadcrumb: React.FunctionComponent<IBreadcrumbProps> = memo((props) => {

  if (!props.subPath || !props.breadcrumb) return (<div className="breadcrumb"></div>);

  // used for reading current location
  // const history = useContext(HistoryContext);

  return (
    <div className={props.subPath.length >= 28 ? "breadcrumb breadcrumb--long" : "breadcrumb"}>
      {
        props.breadcrumb.map((item, index) => {
          let name = item.split("/")[item.split("/").length - 1];

          // instead of nothing
          if (index === 0) {
            name = "Home";
          }

          // For the home page
          if (item === props.subPath) {
            return (<span key={item}><Link to={new URLPath().updateFilePath("history.location.search", item)}>{name}</Link></span>);
          }

          return (<span key={item}><Link to={new URLPath().updateFilePath("history.location.search", item)}>{name}</Link> <span> »</span> </span>);
        })
      }
      {props.subPath.split("/")[props.subPath.split("/").length - 1]}
    </div>
  );
});

export default Breadcrumb;