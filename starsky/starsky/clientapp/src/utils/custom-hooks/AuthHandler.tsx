import * as React from "react";
/** Custom types */
import { UserAuth } from "../../custom-types";
import { DEFAULT_USER_AUTH } from '../Consts';
import useErrorHandler from './ErrorHandler';



const useAuthHandler = (initialState: UserAuth) => {
  const [auth, setAuth] = React.useState(initialState);
  const { error, showError } = useErrorHandler(null);

  React.useEffect(() => {

    var storageItem = localStorage.getItem("UserAuth");
    if (storageItem) {
      setAuthStatus(JSON.parse(storageItem));
    }
    const getAccountStatus = async () => {

      const response = await fetch("/account?json=true", {
        credentials: "include",
        method: "GET",
      });
      var status: UserAuth = { connectionError: false, login: false, username: '' };
      if (!response) {
        status.connectionError = true;
      }
      else if (response.status === 401) {
        status.login = false;
      }
      else {
        response.json().then(function (json) {
          status.username = json["name"];
          status.login = true;
          if (status.login !== auth.login) {
            setAuthStatus(status);
          }
        });
      }
      setAuthStatus(status);
    };

    getAccountStatus();
  }, []);

  const setUnauthStatus = () => {
    if (localStorage.getItem("UserAuth")) {
      localStorage.removeItem("UserAuth");
    }
    setAuth(DEFAULT_USER_AUTH);
  };

  const setAuthStatus = (userAuth: UserAuth) => {
    if (userAuth.login === false) {
      setUnauthStatus();
    }
    else {
      localStorage.setItem("UserAuth", JSON.stringify(userAuth));
      setAuth(userAuth);
    }
  };


  return {
    auth,
    setAuthStatus,
    setUnauthStatus
  };
};

export default useAuthHandler;
