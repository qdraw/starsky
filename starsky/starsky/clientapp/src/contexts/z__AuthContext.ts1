import * as React from "react";
/** Custom types */
import { UserAuth } from "../custom-types";
/** Utils */
import { DEFAULT_USER_AUTH } from "../utils/Consts";
import useAuthHandler from '../utils/custom-hooks/AuthHandler';


interface IAuthContextInterface {
  auth: UserAuth;
  setAuthStatus: (userAuth: UserAuth) => void;
  setUnauthStatus: () => void;
}

export const authContext = React.createContext<IAuthContextInterface>({
  auth: DEFAULT_USER_AUTH,
  setAuthStatus: () => { },
  setUnauthStatus: () => { }
});

const { Provider } = authContext;

const AuthProvider: React.FC<{ children: React.ReactNode }> = ({
  children
}) => {

  const { auth, setAuthStatus, setUnauthStatus } = useAuthHandler(DEFAULT_USER_AUTH);

  return (
    <Provider value={{ auth, setAuthStatus, setUnauthStatus }}>
      {children}
    </Provider>
  );
};

export default AuthProvider;
