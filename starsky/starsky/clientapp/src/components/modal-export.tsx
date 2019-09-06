import React, { memo } from 'react';
import useInterval from '../hooks/use-interval';
import FetchGet from '../shared/fetch-get';
import FetchPost from '../shared/fetch-post';
import { Query } from '../shared/query';
import Modal from './modal';

interface IModalTrashProps {
  isOpen: boolean;
  select: Array<string> | undefined;
  handleExit: Function;
}
// { isModalExportOpen ? <ModalExport handleExit={() => setModalExportOpen(!isModalExportOpen)} select={[detailView.subPath]} isOpen={isModalExportOpen} /> : null }

enum ProcessingState {
  default,
  server,
  ready,
  fail
}

const ModalExport: React.FunctionComponent<IModalTrashProps> = memo((props) => {

  const [isProcessing, setProcessing] = React.useState(ProcessingState.default);
  const [createZipKey, setCreateZipKey] = React.useState("");

  async function postZip(isThumbnail: boolean) {
    if (!props.select) {
      setProcessing(ProcessingState.fail);
      return;
    }
    /*
    f	/__starsky/0001-readonly/4.jpg;/__starsky/0001-readonly/3.jpg
    json	true
    thumbnail	false
    collections	true
    */
    var bodyParams = new URLSearchParams();

    var selectString = "";
    for (let index = 0; index < props.select.length; index++) {
      const element = props.select[index];
      if (index === 0) {
        selectString = element;
        continue;
      }
      selectString += ";" + element;
    }

    bodyParams.set("f", selectString);
    bodyParams.set("json", "true");
    bodyParams.set("thumbnail", isThumbnail.toString());
    bodyParams.set("collections", "true");
    setProcessing(ProcessingState.server);

    var zipKey = await FetchPost(new Query().UrlExportPostZipApi(), bodyParams.toString());
    setCreateZipKey(zipKey);
  }

  useInterval(async () => {
    if (isProcessing !== ProcessingState.server) return;
    var result = await FetchGet(new Query().UrlExportZipApi(createZipKey, true));
    if (result.statusCode === 200) {
      setProcessing(ProcessingState.ready);
      return;
    }
    if (result.statusCode !== 206) {
      setProcessing(ProcessingState.fail);
    }
  }, 1500)

  return (<Modal
    id="detailview-export-modal"
    isOpen={props.isOpen}
    handleExit={() => {
      props.handleExit()
    }}>

    <div className="modal content--subheader">Exporteer selectie</div>
    <div className="modal content--text">

      {isProcessing === ProcessingState.default && props.select && props.select.length === 1 ? <>
        <a href={new Query().UrlDownloadPhotoApi(props.select[0], false)} download target="_blank" className="btn btn--info">Orgineel</a>
        <a href={new Query().UrlDownloadPhotoApi(props.select[0], true)} download target="_blank" className="btn btn--default">Thumbnail</a>
      </> : null}

      {isProcessing === ProcessingState.default && props.select && props.select.length >= 2 ? <>
        <a onClick={() => {
          postZip(false)
        }} className="btn btn--info">Orginelen</a>

        <button onClick={() => {
          postZip(true)
        }} className="btn btn--default">Thumbnails</button>
      </> : null}

      {isProcessing === ProcessingState.server ? <>Loading
      </> : null}

      {isProcessing === ProcessingState.fail ? <>Er is iets mis gegaan met exporteren
      </> : null}

      {isProcessing === ProcessingState.ready ? <>
        <a className="btn btn--default" href={new Query().UrlExportZipApi(createZipKey, false)} download target="_blank">
          Start je download
        </a>
      </> : null}
    </div>
  </Modal>)


});

export default ModalExport