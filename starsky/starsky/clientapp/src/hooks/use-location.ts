import { History } from 'history';
import { useEffect, useState } from 'react';

function useLocation(history: History<any>) {
  const [location, setLocation] = useState(history.location);
  useEffect(
    () => {
      const unListen = history.listen(location => setLocation(location));
      return () => unListen();
    },
    [history],
  );
  return location;
}

export default useLocation;
