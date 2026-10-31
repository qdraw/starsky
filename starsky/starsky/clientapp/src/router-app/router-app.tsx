import { RouterProvider, createBrowserRouter } from "react-router-dom";
import { useTenantLastUsed } from "../hooks/use-tenants/use-tenants";
import { GlobalShortcuts } from "../shared/global-shortcuts/global-shortcuts";
import { RoutesConfig } from "./routes-config";

export const Router = createBrowserRouter(RoutesConfig);

const RouterApp = () => {
  GlobalShortcuts();
  // remember the tenant of this page for the next login
  useTenantLastUsed();
  return <RouterProvider router={Router} />;
};

export default RouterApp;
