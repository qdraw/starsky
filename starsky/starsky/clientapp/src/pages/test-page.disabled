import { RouteComponentProps } from '@reach/router';
import React, { FunctionComponent } from 'react';
import MenuDetailView from '../components/menu-detailview';
import ModalMoveFile from '../components/modal-move-file';
import { DetailViewContext, DetailViewContextProvider } from '../contexts/detailview-context';
import useFileList from '../hooks/use-filelist';
import { IDetailView } from '../interfaces/IDetailView';


interface ITestPageProps { }

const TestPage: FunctionComponent<RouteComponentProps<ITestPageProps>> = (props) => {

  const InsertPayload: FunctionComponent<RouteComponentProps<ITestPageProps>> = (props) => {

    var usesFileList = useFileList("f=/__starsky/01-dif/20180721_153248.jpg");
    console.log(usesFileList);

    const detailView: IDetailView | undefined = usesFileList ? usesFileList.detailView : undefined;

    let { state, dispatch } = React.useContext(DetailViewContext);

    if (!detailView) return (<>Not found</>);
    dispatch({ type: 'reset', payload: detailView });


    if (!state.fileIndexItem) return (<>Not found</>);

    console.log(detailView);

    return (
      <>
        <MenuDetailView />
        <ModalMoveFile parentDirectory="/__starsky/01-dif/" selectedSubPath="/__starsky/01-dif/20180721_153248.jpg" isOpen={true} handleExit={() => { }} />
      </>
    )
  }

  return (
    <DetailViewContextProvider>
      <InsertPayload></InsertPayload>
    </DetailViewContextProvider>
  )
}

export default TestPage;
