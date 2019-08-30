import React from 'react';
import { ArchiveContext } from '../contexts/archive-context';

export function ArchiveDisplay() {
  let { state, dispatch } = React.useContext(ArchiveContext);
  console.log(state);
  if (!state) {
    return (<></>)
  }

  return <div>{`The current count is${state.subPath}  ${state}`}</div>
}

export function ArchiveUpdate() {
  let { state, dispatch } = React.useContext(ArchiveContext);
  if (!dispatch) {
    return (<>dd</>)
  }
  return (
    <>
      <button onClick={() => dispatch({ type: 'add' })}>
        add
    </button>
      <button onClick={() => dispatch({ type: 'update', tags: 'test', description: '', title: '', select: ["20170713_124839-eneco-low-alt.jpg"] })}>
        update
    </button>
    </>
  )
}

