import { RouterProvider, createBrowserRouter } from "react-router-dom";
import ContentPage from "../pages/content-page";
import NotFoundPage from "../pages/not-found-page";
import SearchPage from "../pages/search-page";

export const Router = createBrowserRouter([
  { path: "/", element: <ContentPage /> },
  { path: "starsky/", element: <ContentPage /> },
  { path: "search", element: <SearchPage /> },
  { path: "starsky/search", element: <SearchPage /> },
  { path: "*", element: <NotFoundPage /> }
]);

const RouterApp = () => <RouterProvider router={Router} />;

export default RouterApp;

// const RouterApp = () => {
//   const history = createBrowserHistory();

//   return (
//     <CustomRouter history={history}>
//       <Routes>
//         <Route path="/" element={<ContentPage />} />
//         <Route path="starsky/" element={<ContentPage />} />

//         <Route path="search" element={<SearchPage />} />
//         <Route path="starsky/search" element={<SearchPage />} />

//         <Route path="trash" element={<TrashPage />} />
//         <Route path="starsky/trash" element={<TrashPage />} />

//         <Route path="import" element={<ImportPage />} />
//         <Route path="starsky/import" element={<ImportPage />} />

//         <Route path="login" element={<LoginPage />} />
//         <Route path="starsky/account/login" element={<LoginPage />} />

//         <Route path="account/register" element={<AccountRegisterPage />} />
//         <Route
//           path="starsky/account/register"
//           element={<AccountRegisterPage />}
//         />

//         <Route path="preferences" element={<PreferencesPage />} />
//         <Route path="starsky/preferences" element={<PreferencesPage />} />

//         <Route path="*" element={<NotFoundPage />} />
//       </Routes>
//     </CustomRouter>
//   );
// };
