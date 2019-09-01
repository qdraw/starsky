
import { RouteComponentProps } from '@reach/router';
import React, { FunctionComponent } from 'react';
import Modal from '../components/modal';



interface ISearchPageProps {
}

const Search: FunctionComponent<RouteComponentProps<ISearchPageProps>> = (props) => {
  const [isModalOpen, setModalOpen] = React.useState(false);

  // for some reason "name" here is possibly undefined
  if (props && props.location && props.navigate) {
    // props.location.search = "/?sidebar=true";
    // props.navigate("/some/where", { replace: true });


    return (
      <><Modal
        id="modal-root"
        root="root"
        isOpen={isModalOpen}
        handleExit={() => setModalOpen(false)}
      >
        <div className={`modal-bg ${isModalOpen ? " modal-bg--open" : ""}`}>
          <div
            className={`modal-content ${
              isModalOpen ? " modal-content--show" : ""
              }`}
          >
            <ul>
              <li>Exit button is immediately focused</li>
              <li>
                Current scroll position is cached and overlayed content is
                unscrollable
              </li>
              <li>Focusable elements overlayed are now unfocusable</li>
              <li>Click esc to close modal</li>
            </ul>
          </div>
        </div>
      </Modal>
        <button className="button" onClick={() => setModalOpen(true)}>
          Toggle Modal
        </button>
      </>
    );
  }
  return null;
}


export default Search;
