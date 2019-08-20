import { History } from 'history';
import React from 'react';
import { Routes } from 'universal-router';
import { Provider as HistoryProvider } from '../contexts/history-contexts';
import useRouter from '../hooks/use-router';


export interface Props {
  routes: Routes<any, { default: React.ComponentType }>;
  history: History;
  fallback: NonNullable<React.ReactNode> | null;
}

function Router(props: Props) {
  const Component = useRouter(props.routes, props.history);
  return (
    <HistoryProvider value={props.history}>
      <React.Suspense fallback={props.fallback}>
        <Component />
      </React.Suspense>
    </HistoryProvider>
  );
}

export default Router;

