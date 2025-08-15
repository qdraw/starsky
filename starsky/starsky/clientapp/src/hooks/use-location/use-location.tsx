import { useNavigate, useLocation as useRouterLocation } from "react-router-dom";

import { INavigateState } from "../../interfaces/INavigateState";
import { ILocationObject } from "./interfaces/ILocationObject";
import { IUseLocation } from "./interfaces/IUseLocation";

function useLocation(): IUseLocation {
  const locationReactRouter = useRouterLocation();
  const navigate = useNavigate();

  const state = (locationReactRouter.state as INavigateState) ?? null;
  console.log(state);

  const location = locationReactRouter as unknown as ILocationObject;

  return { location, state, navigate };
}

export default useLocation;
