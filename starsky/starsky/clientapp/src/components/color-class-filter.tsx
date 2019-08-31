import { Link } from '@reach/router';
import React, { memo } from 'react';
import useLocation from '../hooks/use-location';
import { URLPath } from '../shared/url-path';

//         <ColorClassFilter itemsCount={this.props.collectionsCount} subPath={this.props.subPath} colorClassFilterList={this.props.colorClassFilterList} colorClassUsage={this.props.colorClassUsage}></ColorClassFilter>
export interface IColorClassProp {
  subPath: string;
  colorClassFilterList: Array<number>;
  colorClassUsage: Array<number>;
  itemsCount?: number;
}

const ColorClassFilter: React.FunctionComponent<IColorClassProp> = memo((props) => {
  // used for reading current location
  var history = useLocation();

  var colorContent: string[] = [
    "Geen",
    "Paars",
    "Rood",
    "Oranje",
    "Geel",
    "Groen",
    "Azuur",
    "Blauw",
    "Grijs",
    "Herstel Filter"
  ];

  function cleanColorClass(): string {
    if (!props.subPath) return "/";
    var urlObject = new URLPath().StringToIUrl(history.location.search);
    urlObject.colorClass = [];
    return new URLPath().IUrlToString(urlObject);
  }

  function updateColorClass(item: number): string {
    var urlObject = new URLPath().StringToIUrl(history.location.search);
    if (!urlObject.colorClass) {
      urlObject.colorClass = [];
    }

    if (!urlObject.colorClass || urlObject.colorClass.indexOf(item) === -1) {
      urlObject.colorClass.push(item)
    }
    else {
      var index = urlObject.colorClass.indexOf(item);
      if (index !== -1) urlObject.colorClass.splice(index, 1);
    }
    return new URLPath().IUrlToString(urlObject);
  }

  let resetButton = <Link to={cleanColorClass()} className="btn colorclass colorclass--reset">{colorContent[9]}</Link>;
  let resetButtonDisabled = <div className="btn colorclass colorclass--reset disabled">{colorContent[9]}</div>;

  // there is no content ?
  if (props.colorClassUsage.length === 1 && props.colorClassFilterList.length >= 1) return (<div className="colorclass colorclass--filter"> {resetButton}</div>);

  if (props.itemsCount === 0 || props.colorClassUsage.length === 1) return (<></>);
  return (<div className="colorclass colorclass--filter">
    {
      props.colorClassFilterList.length !== 0 ? resetButton : resetButtonDisabled
    }
    {
      props.colorClassUsage.map((item, index) => (
        item >= 0 && item <= 8 ? <Link key={item} to={updateColorClass(item)}
          className={props.colorClassFilterList.indexOf(item) >= 0 ? "btn btn--default colorclass colorclass--" + item + " active" : "btn colorclass colorclass--" + item}>
          <label></label><span>{colorContent[item]}</span> </Link>
          : <span key={item}></span>
      ))
    }
  </div>);
});

export default ColorClassFilter
