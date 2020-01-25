import { Router } from "@reach/router";
import React from "react";
import AccountRegisterPage from '../pages/account-register-page';
import ContentPage from '../pages/content-page';
import ImportPage from '../pages/import-page';
import LoginPage from '../pages/login-page';
import NotFoundPage from '../pages/not-found-page';
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
    <LoginPage path="account/login" />
    <AccountRegisterPage path="account/register" />
    <NotFoundPage default />
  </Router>
);


export default RouterApp;