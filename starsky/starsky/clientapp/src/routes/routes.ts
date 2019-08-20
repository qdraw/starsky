import { Routes } from 'universal-router';

const routes: Routes<any, { default: any }> = [
  {
    path: '/index.html',
    action: () => import('../pages/first-page'),
  },
  {
    path: '/',
    action: () => import('../pages/first-page'),
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
