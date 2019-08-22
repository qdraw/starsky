import { Routes } from 'universal-router';

const routes: Routes<any, { default: any }> = [
  {
    path: '/beta',
    action: () => import('../pages/content-page'),
  },
  {
    path: '/',
    action: () => import('../pages/content-page'),
  },
  {
    path: '/index.html#search',
    action: () => import('../pages/search'),
  },
  {
    path: '/search',
    action: () => import('../pages/search'),
  },
  {
    path: '(.*)', // wildcard route (must go last)
    action: () => import('../pages/error'),
  }
];

export default routes;
