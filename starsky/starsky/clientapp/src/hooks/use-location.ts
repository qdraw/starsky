import { Location, NavigateFunction, useNavigate } from "react-router-dom";
import history from "../shared/global-history/global-history";

export interface IUseLocation {
  location: Location;
  navigate: NavigateFunction;
}

const useLocation = () => {
  const navigate = useNavigate();
  const result = {
    navigate,
    location: history.location as Location
  } as IUseLocation;

  return result;
};

export default useLocation;
