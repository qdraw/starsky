import { Router } from "@reach/router";
import React from "react";
import ContentPage from '../pages/content-page';
import Search from '../pages/search';

const RouterApp = () => (
  <Router>
    <ContentPage path="/" />
    <ContentPage path="beta" />
    <Search path="search" />
    <Search path="beta/search" />
  </Router>
);


export default RouterApp;