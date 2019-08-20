import * as React from "react";
/** Context */
import { authContext } from "../contexts/AuthContext";
import ConnectionError from '../pages/connection-error';
import Login from "./login";
import MediaContent from './media-content';

function RootContainer() {
  const { auth } = React.useContext(authContext);
  return (
    <div>
      {auth.connectionError ? <ConnectionError /> : null}
      {auth.login && !auth.connectionError ? <MediaContent /> : null}
      {!auth.login && !auth.connectionError && <Login />}
    </div>
  );
}

export default RootContainer;
