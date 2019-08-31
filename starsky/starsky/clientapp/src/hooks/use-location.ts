// import { History } from 'history';
// import { useEffect, useState } from 'react';

// function useLocation(history: History<any>) {
//   const [location, setLocation] = useState(history.location);
//   useEffect(
//     () => {
//       const unListen = history.listen(location => setLocation(location));
//       return () => unListen();
//     },
//     [history],
//   );
//   return location;
// }

// export default useLocation;

import { globalHistory } from '@reach/router';
import { useEffect, useState } from 'react';

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
  }, []);

  return state;
};

export default useLocation;
// credits: https://github.com/reach/router/issues/203#issuecomment-453941158