import React, { useEffect, useState } from 'react';
import { ArchiveContext } from '../contexts/archive-context';
import useInterval from '../hooks/use-interval';
import useLocation from '../hooks/use-location';
import { IArchiveProps } from '../interfaces/IArchiveProps';
import { CastToInterface } from '../shared/cast-to-interface';
import FetchGet from '../shared/fetch-get';
import FetchPost from '../shared/fetch-post';
import { URLPath } from '../shared/url-path';
import Modal from './modal';
import Preloader from './preloader';
import SwitchButton from './switch-button';

interface IModalDisplayOptionsProps {
  isOpen: boolean;
  handleExit: Function;
  parentFolder?: string;
}

const ModalDisplayOptions: React.FunctionComponent<IModalDisplayOptionsProps> = (props) => {

  // preloading icon
  const [isLoading, setIsLoading] = useState(false);

  var history = useLocation();
  let { dispatch } = React.useContext(ArchiveContext);

  // the default is true
  const [collections, setCollections] = React.useState(new URLPath().StringToIUrl(history.location.search).collections !== false);

  /** update when changing values and search */
  useEffect(() => {
    setCollections(new URLPath().StringToIUrl(history.location.search).collections !== false);
  }, [collections, history.location.search])

  function toggleCollections() {
    var urlObject = new URLPath().StringToIUrl(history.location.search);
    // set the default option
    if (urlObject.collections === undefined) urlObject.collections = true;
    urlObject.collections = !urlObject.collections;
    setCollections(urlObject.collections);
    history.navigate(new URLPath().IUrlToString(urlObject), { replace: true })
  }

  var singleItem = localStorage.getItem("issingleitem");
  const [isSingleItem, setIsSingleItem] = React.useState(singleItem === "false");

  function toggleSlowFiles() {
    setIsSingleItem(!isSingleItem);
    localStorage.setItem("issingleitem", isSingleItem.toString())
  }

  function removeCache() {
    setIsLoading(true);

    var parentFolder = props.parentFolder ? props.parentFolder : "/";
    FetchGet("/api/RemoveCache?json=true&f=" + new URLPath().encodeURI(parentFolder)).then((_) => {
      setTimeout(() => {
        FetchGet("/api/index/?f=" + new URLPath().encodeURI(parentFolder)).then((connectionResult) => {
          var removeCacheResult = new CastToInterface().MediaArchive(connectionResult.data);
          var payload = removeCacheResult.data as IArchiveProps;
          if (payload.fileIndexItems) {
            dispatch({ type: 'force-reset', payload });
          }
          props.handleExit();
        });
      }, 500);
    });
  }

  const [geoSyncPercentage, setGeoSyncPercentage] = useState(0);

  function geoSync() {
    var parentFolder = props.parentFolder ? props.parentFolder : "/";
    var bodyParams = new URLSearchParams();
    bodyParams.set("f", parentFolder);

    FetchPost("/api/geo/sync", bodyParams.toString()).then((anyData) => {
    });
  }

  function fetchGeoSyncStatus() {
    var parentFolder = props.parentFolder ? props.parentFolder : "/";
    FetchGet("/api/geo/status/?f=" + new URLPath().encodeURI(parentFolder)).then((anyData) => {

      if (anyData.statusCode !== 200 || !anyData.data) {
        setGeoSyncPercentage(-1);
        return;
      }

      if (anyData.data.current === 0 && anyData.data.total === 0) {
        setGeoSyncPercentage(0);
        return;
      }
      setGeoSyncPercentage(anyData.data.current / anyData.data.total * 100);
    });
  }

  useInterval(() => fetchGeoSyncStatus(), 3333);

  function forceSync() {
    var parentFolder = props.parentFolder ? props.parentFolder : "/";
    setIsLoading(true);
    FetchGet("/sync/?f=" + new URLPath().encodeURI(parentFolder)).then((_) => {
      setTimeout(() => {
        FetchGet("/api/index/?f=" + new URLPath().encodeURI(parentFolder)).then((connectionResult) => {
          var forceSyncResult = new CastToInterface().MediaArchive(connectionResult.data);
          var payload = forceSyncResult.data as IArchiveProps;
          if (payload.fileIndexItems) {
            dispatch({ type: 'force-reset', payload });
          }
          props.handleExit();
        });
      }, 7000);
    });
  }

  return (<Modal
    id="detailview-export-modal"
    isOpen={props.isOpen}
    handleExit={() => {
      props.handleExit()
    }}>
    {isLoading ? <Preloader isDetailMenu={false} isOverlay={true} /> : ""}

    <div className="modal content--subheader">Weergave opties</div>
    <div className="content--text">
      <SwitchButton isOn={!collections} isEnabled={true} leftLabel="Collecties aan" onToggle={() => toggleCollections()} rightLabel="Per bestand (uit)" />
    </div>
    <div className="modal content--subheader">
      <SwitchButton isOn={isSingleItem} isEnabled={true} leftLabel="Alles inladen" rightLabel="Klein inladen" onToggle={() => toggleSlowFiles()} />
    </div>
    <div className="modal content--text">
    </div>
    <div className="modal content--header">
      <button className="btn btn--info" onClick={() => forceSync()}>Forceer sync</button>
      <button className="btn btn--info" onClick={() => removeCache()}>Vernieuw</button>
      <button className="btn btn--info btn--percentage" onClick={() => geoSync()}>Geo sync {geoSyncPercentage}%</button>

    </div>
  </Modal>)
}

export default ModalDisplayOptions;
