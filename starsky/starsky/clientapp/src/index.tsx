import 'abortcontroller-polyfill/dist/abortcontroller-polyfill-only'; // for the feature
import 'core-js/features/dom-collections/for-each'; // queryselector.forEach
import 'core-js/features/object'; // Object.entries is not a function
import 'core-js/features/promise'; // Yes I promise
// import 'core-js/features/set';
import 'core-js/features/string/match'; // event.key.match
// import 'core-js/features/symbol';
import 'core-js/features/url-search-params'; // new UrlSearchParams
import React from 'react';
import * as ReactDOM from "react-dom";
import RouterApp from './routes/router';
import * as serviceWorker from "./service-worker";
import './style/css/00-index.css';

ReactDOM.render(
  <RouterApp />,
  document.getElementById('root'),
);

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister();
