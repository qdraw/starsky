import { Router } from "@reach/router";
import React from "react";
import ContentPage from '../pages/content-page';
import ImportPage from '../pages/import';
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
    <ImportPage path="starsky/import" />
    <ImportPage path="import" />
  </Router>
);


export default RouterApp;