import { Link } from '@reach/router';
import React, { memo, useEffect, useState } from 'react';
import { ArchiveContext } from '../../../contexts/archive-context';
import useGlobalSettings from '../../../hooks/use-global-settings';
import useLocation from '../../../hooks/use-location';
import { newIArchive } from '../../../interfaces/IArchive';
import { Language } from '../../../shared/language';
import { SelectCheckIfActive } from '../../../shared/select-check-if-active';
import { URLPath } from '../../../shared/url-path';
import Preloader from '../../atoms/preloader/preloader';

//  <ColorClassFilter itemsCount={this.props.collectionsCount} subPath={this.props.subPath} 
// colorClassActiveList={this.props.colorClassActiveList} colorClassUsage={this.props.colorClassUsage}></ColorClassFilter>
export interface IColorClassProp {
  subPath: string;
  colorClassActiveList: Array<number>;
  colorClassUsage: Array<number>;
  itemsCount?: number;
}

const ColorClassFilter: React.FunctionComponent<IColorClassProp> = memo((props) => {

  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);

  const colorContent: Array<string> = [
    language.text("Kleurloos", "Colorless"),
    language.text("Paars", "Purple"),
    language.text("Rood", "Red"),
    language.text("Oranje", "Orange"),
    language.text("Geel", "Yellow"),
    language.text("Groen", "Green"),
    language.text("Azuur", "Azure"),
    language.text("Blauw", "Blue"),
    language.text("Grijs", "Grey"),
    language.text("Herstel filter", "Reset filter"),
  ];

  // used for reading current location
  var history = useLocation();

  let { state } = React.useContext(ArchiveContext);
  // props is used as default, state only for update
  if (!state) state = { ...newIArchive(), colorClassUsage: props.colorClassUsage }
  const [colorClassUsage, setIsColorClassUsage] = useState(props.colorClassUsage);

  useEffect(() => {
    if (!state.colorClassUsage) return;
    setIsColorClassUsage(state.colorClassUsage);
    // it should not update when the prop are changing
    // eslint-disable-next-line
  }, [state.colorClassUsage])

  const [isLoading, setIsLoading] = useState(false);
  // When change-ing page the loader should be gone
  useEffect(() => {
    setIsLoading(false);
  }, [props.colorClassActiveList])

  function cleanColorClass(): string {
    if (!props.subPath) return "/";
    var urlObject = new URLPath().StringToIUrl(history.location.search);
    urlObject.colorClass = [];
    return new URLPath().IUrlToString(urlObject);
  }

  function getFilterUrlColorClass(item: number): string {
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

    new SelectCheckIfActive().IsActive(urlObject.select, urlObject.colorClass, state.fileIndexItems)

    return new URLPath().IUrlToString(urlObject);
  }


  let resetButton = <Link to={cleanColorClass()} className="btn colorclass colorclass--reset">{colorContent[9]}</Link>;
  let resetButtonDisabled = <div className="btn colorclass colorclass--reset disabled">{colorContent[9]}</div>;

  // there is no content ?
  if (props.colorClassUsage.length === 1 && props.colorClassActiveList.length >= 1) return (
    <div className="colorclass colorclass--filter"> {resetButton}</div>
  );

  if (props.itemsCount === 0 || colorClassUsage.length === 1) return (<></>);
  return (<div className="colorclass colorclass--filter">
    {isLoading ? <Preloader isDetailMenu={false} isOverlay={true} /> : null}
    {
      props.colorClassActiveList.length !== 0 ? resetButton : resetButtonDisabled
    }
    {
      colorClassUsage.map((item) => (
        item >= 0 && item <= 8 ? <Link onClick={() =>
          setIsLoading(true)
        } key={item} to={getFilterUrlColorClass(item)}
          className={props.colorClassActiveList.indexOf(item) >= 0 ?
            "btn btn--default colorclass colorclass--" + item + " active" : "btn colorclass colorclass--" + item}>
          <label /><span>{colorContent[item]}</span> </Link>
          : <span key={item} />
      ))
    }
  </div >);
});

export default ColorClassFilter
