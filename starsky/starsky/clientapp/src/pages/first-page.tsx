import React from 'react';
import RootContainer from '../containers/root-container';
import AuthContextProvider from "../contexts/AuthContext";

function FirstPage() {
  return (
    <AuthContextProvider>
      <RootContainer />
    </AuthContextProvider>
  );
}

export default FirstPage;
