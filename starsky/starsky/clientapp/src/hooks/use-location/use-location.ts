import { useEffect, useState } from "react";
import { Router } from "../../router-app/router-app";
import { IUseLocation } from "./interfaces/IUseLocation";
import { NavigateFn } from "./shared/navigate-fn";

const useLocation = () => {
  // eslint-disable-next-line react-hooks/exhaustive-deps
  const initialState: IUseLocation = {
    location: window.location,
    navigate: NavigateFn
  };

  const [state, setState] = useState(initialState);
  useEffect(() => {
    const removeListener = Router.subscribe((params) => {
      const { location } = params;
      const newState = { ...initialState, ...location };
      setState(newState);
    });
    return () => {
      removeListener();
    };
  }, [initialState]);

  return state;
};

export default useLocation;
// credits: https://github.com/reach/router/issues/203#issuecomment-453941158
