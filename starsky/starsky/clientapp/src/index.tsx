import React from 'react';
import * as ReactDOM from "react-dom";
import Preloader from './components/preloader';
import Router from './routes/router';
import routes from './routes/routes';
import * as serviceWorker from "./service-worker";
import history from './shared/history';
import './style/css/00-index.css';

ReactDOM.render(
  <Router history={history} routes={routes} fallback={<Preloader isOverlay={true} isDetailMenu={false}></Preloader>} />,
  document.getElementById('root'),
);

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister();
