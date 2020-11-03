import { Router } from "@reach/router";
import React from "react";
import AccountRegisterPage from '../pages/account-register-page';
import ContentPage from '../pages/content-page';
import ImportPage from '../pages/import-page';
import LoginPage from '../pages/login-page';
import NotFoundPage from '../pages/not-found-page';
import PreferencesPage from '../pages/preferences-page';
import SearchPage from '../pages/search-page';
import TrashPage from '../pages/trash-page';

const RouterApp = () => (
  <Router>
    <ContentPage path="/" />
    <ContentPage path="starsky" />

    <SearchPage path="search" />
    <SearchPage path="starsky/search" />

    <TrashPage path="trash" />
    <TrashPage path="starsky/trash" />

    <ImportPage path="import" />
    <ImportPage path="starsky/import" />

    <LoginPage path="account/login" />
    <LoginPage path="starsky/account/login" />

    <AccountRegisterPage path="account/register" />
    <AccountRegisterPage path="starsky/account/register" />

    <PreferencesPage path="preferences" />
    <PreferencesPage path="starsky/preferences" />

    <NotFoundPage default />
  </Router>
);


export default RouterApp;