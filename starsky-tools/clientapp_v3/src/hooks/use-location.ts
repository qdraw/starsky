import { globalHistory, HistoryLocation, NavigateFn } from '@reach/router';
import { useEffect, useState } from 'react';

export interface IUseLocation {
  location: HistoryLocation,
  navigate: NavigateFn
}

const useLocation = () => {
  const initialState = {
    location: globalHistory.location,
    navigate: globalHistory.navigate,
  };

  const [state, setState] = useState(initialState);
  useEffect(() => {
    const removeListener = globalHistory.listen(params => {
      const { location } = params;
      const newState = Object.assign({}, initialState, { location });
      setState(newState);
    });
    return () => {
      removeListener();
    };
  }, [initialState]);

  return state as IUseLocation;
};

export default useLocation;
// credits: https://github.com/reach/router/issues/203#issuecomment-453941158