import { RouteObject } from "react-router-dom";
import { AccountRegisterPage } from "../pages/account-register-page";
import { ContentPage } from "../pages/content-page";
import ImportPage from "../pages/import-page";
import { LoginPage } from "../pages/login-page";
import { MyTenantsPage } from "../pages/my-tenants-page";
import { NotFoundPage } from "../pages/not-found-page";
import { PreferencesPage } from "../pages/preferences-page";
import { SearchPage } from "../pages/search-page";
import { TrashPage } from "../pages/trash-page";

export const RoutesConfig: RouteObject[] = [
  {
    path: "/",
    element: <ContentPage />,
    errorElement: <NotFoundPage />
  },
  { path: "starsky/", element: <ContentPage /> },
  { path: "-/tenants", element: <MyTenantsPage /> },
  { path: "starsky/-/tenants", element: <MyTenantsPage /> },
  { path: "search", element: <SearchPage /> },
  { path: "starsky/search", element: <SearchPage /> },
  { path: ":tenant/search", element: <SearchPage /> },
  { path: "starsky/:tenant/search", element: <SearchPage /> },
  { path: "trash", element: <TrashPage /> },
  { path: "starsky/trash", element: <TrashPage /> },
  { path: ":tenant/trash", element: <TrashPage /> },
  { path: "starsky/:tenant/trash", element: <TrashPage /> },
  { path: "import", element: <ImportPage /> },
  { path: "starsky/import", element: <ImportPage /> },
  { path: ":tenant/import", element: <ImportPage /> },
  { path: "starsky/:tenant/import", element: <ImportPage /> },
  { path: "login", element: <LoginPage /> },
  { path: "starsky/login", element: <LoginPage /> },
  { path: "account/login", element: <LoginPage /> },
  { path: "starsky/account/login", element: <LoginPage /> },
  { path: ":tenant/account/login", element: <LoginPage /> },
  { path: "starsky/:tenant/account/login", element: <LoginPage /> },
  { path: "account/register", element: <AccountRegisterPage /> },
  { path: "starsky/account/register", element: <AccountRegisterPage /> },
  { path: ":tenant/account/register", element: <AccountRegisterPage /> },
  { path: "starsky/:tenant/account/register", element: <AccountRegisterPage /> },
  { path: "preferences", element: <PreferencesPage /> },
  { path: "starsky/preferences", element: <PreferencesPage /> },
  { path: ":tenant/preferences", element: <PreferencesPage /> },
  { path: "starsky/:tenant/preferences", element: <PreferencesPage /> },
  { path: ":tenant", element: <ContentPage /> },
  { path: "starsky/:tenant", element: <ContentPage /> },
  { path: "*", element: <NotFoundPage /> }
];
