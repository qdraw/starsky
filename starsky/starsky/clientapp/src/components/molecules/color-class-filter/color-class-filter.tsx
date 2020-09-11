import { Link } from '@reach/router';
import React, { memo, useEffect, useState } from 'react';
import useGlobalSettings from '../../../hooks/use-global-settings';
import useLocation from '../../../hooks/use-location';
import { Language } from '../../../shared/language';
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

  const [colorClassUsage] = useState(props.colorClassUsage);

  // const [colorClassUsage, setIsColorClassUsage] = useState(props.colorClassUsage);
  // if (!state) state = { ...newIArchive(), colorClassUsage: [] }

  // useEffect(() => {
  //   setIsColorClassUsage(state.colorClassUsage);
  //   // it should not update when the prop are changing
  //   // eslint-disable-next-line
  // }, [state.colorClassUsage])

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

  function updateColorClass(item: number): string {
    var urlObject = new URLPath().StringToIUrl(history.location.search);

    if (!urlObject.colorClass) {
      urlObject.colorClass = [];
    }

    //  checkIfSelectIsActive(urlObject.select, urlObject.colorClass, state.fileIndexItems)

    if (!urlObject.colorClass || urlObject.colorClass.indexOf(item) === -1) {
      urlObject.colorClass.push(item)
    }
    else {
      var index = urlObject.colorClass.indexOf(item);
      if (index !== -1) urlObject.colorClass.splice(index, 1);
    }
    return new URLPath().IUrlToString(urlObject);
  }

  // function checkIfSelectIsActive(select: string[] | undefined, colorClassList: number[], fileIndexItems: IFileIndexItem[]) {
  //   if (!select || !colorClassList) return;

  //   // todo merge
  //   // check if file exist in state or remove the selected item from the selection
  //  // CHECK if tests are not timeout due memory issues
  //   colorClassList.forEach(usage => {
  //     const even = (element: IFileIndexItem) => element.colorClass === usage;
  //     if (!state.fileIndexItems.some(even).valueOf()) {
  //       var indexer = colorClassList.indexOf(usage);
  //       state.colorClassUsage.splice(indexer, 1);

  //     }

  //     // console.log(element);
  //   });
  // }

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
        item >= 0 && item <= 8 ? <Link onClick={() => setIsLoading(true)} key={item} to={updateColorClass(item)}
          className={props.colorClassActiveList.indexOf(item) >= 0 ?
            "btn btn--default colorclass colorclass--" + item + " active" : "btn colorclass colorclass--" + item}>
          <label /><span>{colorContent[item]}</span> </Link>
          : <span key={item} />
      ))
    }
  </div>);
});

export default ColorClassFilter
