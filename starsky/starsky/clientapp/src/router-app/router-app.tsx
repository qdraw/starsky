import {
  RouteObject,
  RouterProvider,
  createBrowserRouter
} from "react-router-dom";
import { AccountRegisterPage } from "../pages/account-register-page";
import { ContentPage } from "../pages/content-page";
import ImportPage from "../pages/import-page";
import { LoginPage } from "../pages/login-page";
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
  { path: "search", element: <SearchPage /> },
  { path: "starsky/search", element: <SearchPage /> },
  { path: "trash", element: <TrashPage /> },
  { path: "starsky/trash", element: <TrashPage /> },
  { path: "import", element: <ImportPage /> },
  { path: "starsky/import", element: <ImportPage /> },
  { path: "login", element: <LoginPage /> },
  { path: "starsky/login", element: <LoginPage /> },
  { path: "account/login", element: <LoginPage /> },
  { path: "starsky/account/login", element: <LoginPage /> },
  { path: "account/register", element: <AccountRegisterPage /> },
  { path: "starsky/account/register", element: <AccountRegisterPage /> },
  { path: "preferences", element: <PreferencesPage /> },
  { path: "starsky/preferences", element: <PreferencesPage /> },
  { path: "*", element: <NotFoundPage /> }
];
export const Router = createBrowserRouter(RoutesConfig);

const RouterApp = () => <RouterProvider router={Router} />;

export default RouterApp;
