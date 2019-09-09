import React from 'react';
import useLocation from '../hooks/use-location';
import FetchGet from '../shared/fetch-get';
import { URLPath } from '../shared/url-path';
import Modal from './modal';
import SwitchButton from './switch-button';

interface IModalDisplayOptionsProps {
  isOpen: boolean;
  handleExit: Function;
  parentFolder?: string;
}

const ModalDisplayOptions: React.FunctionComponent<IModalDisplayOptionsProps> = (props) => {

  var history = useLocation();

  const [collections, setCollections] = React.useState(new URLPath().StringToIUrl(history.location.search).collections);

  function toggleCollections() {
    var urlObject = new URLPath().StringToIUrl(history.location.search);
    urlObject.collections = !urlObject.collections;
    setCollections(urlObject.collections);
    history.navigate(new URLPath().IUrlToString(urlObject), { replace: true })
  }

  var singleItem = localStorage.getItem("issingleitem");
  const [isSingleItem, setIsSingleItem] = React.useState(singleItem === "false");

  function toggleSlowFiles() {
    setIsSingleItem(!isSingleItem)
    localStorage.setItem("issingleitem", isSingleItem.toString())
  }

  const [removeCacheEnabled, setRemoveCacheEnabled] = React.useState(true);
  function removeCache() {
    FetchGet("/api/RemoveCache?json=true&f=" + props.parentFolder)
    setRemoveCacheEnabled(false)
  }

  const [forceSyncEnabled, setForceSyncEnabled] = React.useState(true);
  function forceSync() {
    FetchGet("/sync/?f=" + props.parentFolder)
    setForceSyncEnabled(false);
  }

  return (<Modal
    id="detailview-export-modal"
    isOpen={props.isOpen}
    handleExit={() => {
      props.handleExit()
    }}>

    <div className="modal content--subheader">Weergave opties</div>
    <div className="content--text">
      <SwitchButton isOn={!collections} isEnabled={true} leftLabel="Collecties aan" onToggle={() => toggleCollections()} rightLabel="Per bestand"></SwitchButton>
    </div>
    <div className="modal content--subheader">
      <SwitchButton isOn={isSingleItem} isEnabled={true} leftLabel="Alles inladen" rightLabel="Klein inladen" onToggle={() => toggleSlowFiles()}></SwitchButton>
    </div>
    <div className="modal content--text">
    </div>

    <div className="modal content--header">
      <button disabled={!forceSyncEnabled} className="btn btn--info" onClick={() => forceSync()}>Forceer sync</button>
      <button disabled={!removeCacheEnabled} className="btn btn--info" onClick={() => removeCache()}>Vernieuw</button>

    </div>
  </Modal>)
}

export default ModalDisplayOptions;
