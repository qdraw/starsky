import { Router } from "@reach/router";
import React from "react";
import ContentPage from '../pages/content-page';
import SearchPage from '../pages/search-page';
import TrashPage from '../pages/trash-page';

const RouterApp = () => (
  <Router>
    <ContentPage path="/" />
    <ContentPage path="starsky" />
    <SearchPage path="search" />
    <SearchPage path="starsky/search" />
    <TrashPage path="starsky/trash" />
    <TrashPage path="trash" />

  </Router>
);


export default RouterApp;