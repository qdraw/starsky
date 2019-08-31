import React, { memo } from 'react';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import ListImageBox from './list-image-box';

interface ItemListProps {
  fileIndexItems: Array<IFileIndexItem>,
  colorClassUsage: Array<number>,
}
/**
 * A list with links to the items
 */
const ItemListView: React.FunctionComponent<ItemListProps> = memo((props) => {

  // todo: implement feature that saves the scroll height
  // var history = useLocation();
  // useEffect(() => {
  //   var state = history.location.state as INavigateState;
  //   if (!state.fileName) return;

  //   console.log(state.fileName);
  // }, [history.location.state]);

  let items = props.fileIndexItems;
  if (!items) return (<div className="folder">no content</div>);

  return (
    <div className="folder">
      {items.length === 0 ? props.colorClassUsage.length >= 1 ? <div className="warning-box warning-box--left">Er zijn meer items, maar deze vallen buiten je filters</div> : <div className="warning-box"> Er zijn geen foto's in deze map</div> : ""}
      {
        items.map((item, index) => (
          <ListImageBox item={item} key={item.lastEdited}></ListImageBox>
        ))
      }
    </div>)


  // return (
  //   <>
  //     <ArchiveContextConsumer>
  //       {appContext =>
  //         appContext && (
  //           <div className="folder">
  //             {appContext.state.fileIndexItems.length === 0 ? appContext.state.colorClassUsage.length >= 1 ? <div className="warning-box warning-box--left">Er zijn meer items, maar deze vallen buiten je filters</div> : <div className="warning-box"> Er zijn geen foto's in deze map</div> : ""}
  //             {
  //               appContext.state.fileIndexItems.map((item, index) => (
  //                 <ListImageBox item={item} key={item.fileName}></ListImageBox>
  //               ))
  //             }
  //           </div>
  //         )
  //       }
  //     </ArchiveContextConsumer>
  //   </>
  // );
});

export default ItemListView
