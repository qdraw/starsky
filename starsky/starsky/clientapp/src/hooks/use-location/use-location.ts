import { useEffect, useState } from "react";
import { Router } from "../../router-app/router-app";
import { IUseLocation } from "./interfaces/IUseLocation";
import { NavigateFn } from "./internal/navigate-fn";

const useLocation = () => {
  // es_lint-disable-next-line react-hooks/exhaustive-deps // https://github.com/facebook/react/pull/30774
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
