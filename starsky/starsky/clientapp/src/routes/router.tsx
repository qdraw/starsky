import { Router } from "@reach/router";
import React from "react";
import ContentPage from '../pages/content-page';
import SearchPage from '../pages/search-page';

const RouterApp = () => (
  <Router>
    <ContentPage path="/" />
    <ContentPage path="beta" />
    <SearchPage path="search" />
    <SearchPage path="beta/search" />
  </Router>
);


export default RouterApp;