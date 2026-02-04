import { RouterProvider, createBrowserRouter } from "react-router-dom";
import { GlobalShortcuts } from "../shared/global-shortcuts/global-shortcuts";
import { RoutesConfig } from "./routes-config";

export const Router = createBrowserRouter(RoutesConfig);

const RouterApp = () => {
  GlobalShortcuts();
  return <RouterProvider router={Router} />;
};

export default RouterApp;
