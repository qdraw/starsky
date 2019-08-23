import { Router } from "@reach/router";
import React from "react";
import ContentPage from '../pages/content-page';

const RouterApp = () => (
  <Router>
    <ContentPage path="/" />
    <ContentPage path="beta" />
  </Router>
);


export default RouterApp;