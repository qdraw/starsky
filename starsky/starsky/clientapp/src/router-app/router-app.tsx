import { createBrowserHistory } from "history";
import { Route, Routes } from "react-router-dom";
import { CustomRouter } from "../components/atoms/custom-router/custom-router";
import AccountRegisterPage from "../pages/account-register-page";
import ContentPage from "../pages/content-page";
import ImportPage from "../pages/import-page";
import LoginPage from "../pages/login-page";
import NotFoundPage from "../pages/not-found-page";
import PreferencesPage from "../pages/preferences-page";
import SearchPage from "../pages/search-page";
import TrashPage from "../pages/trash-page";

const RouterApp = () => {
  const history = createBrowserHistory();

  return (
    <CustomRouter history={history}>
      <Routes>
        <Route path="/" element={<ContentPage />} />
        <Route path="starsky/" element={<ContentPage />} />

        <Route path="search" element={<SearchPage />} />
        <Route path="starsky/search" element={<SearchPage />} />

        <Route path="trash" element={<TrashPage />} />
        <Route path="starsky/trash" element={<TrashPage />} />

        <Route path="import" element={<ImportPage />} />
        <Route path="starsky/import" element={<ImportPage />} />

        <Route path="login" element={<LoginPage />} />
        <Route path="starsky/account/login" element={<LoginPage />} />

        <Route path="account/register" element={<AccountRegisterPage />} />
        <Route
          path="starsky/account/register"
          element={<AccountRegisterPage />}
        />

        <Route path="preferences" element={<PreferencesPage />} />
        <Route path="starsky/preferences" element={<PreferencesPage />} />

        <Route path="*" element={<NotFoundPage />} />
      </Routes>
    </CustomRouter>
  );
};

export default RouterApp;
