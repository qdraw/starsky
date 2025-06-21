import { useLocation, useNavigate } from "react-router-dom";
import { INavigateState } from "../../interfaces/INavigateState";
import { ILocationObject } from "./interfaces/ILocationObject";
import { IUseLocation } from "./interfaces/IUseLocation";

function useLocationState(): IUseLocation {
  const locationReactRouter = useLocation();
  const navigate = useNavigate();

  const state = (locationReactRouter.state as INavigateState) ?? null;
  console.log(state);

  const location = locationReactRouter as unknown as ILocationObject;

  return { location, state, navigate };
}

export default useLocationState;
