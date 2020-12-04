import "abortcontroller-polyfill/dist/abortcontroller-polyfill-only"; // for the feature
import "core-js/features/array/filter"; // array filter
import "core-js/features/array/some"; // https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Array/some
import "core-js/features/dom-collections/for-each"; // queryselector.forEach
import "core-js/features/object"; // Object.entries is not a function
import "core-js/features/promise"; // Yes I promise
import "core-js/features/string/match"; // event.key.match
import "core-js/features/url-search-params"; // new UrlSearchParams
import React from "react";
import * as ReactDOM from "react-dom";
import RouterApp from "./router-app/router-app";
import "./style/css/00-index.css";

/* used for image policy */
/// <reference path='./index.d.ts'/>

ReactDOM.render(
  <React.StrictMode>
    <RouterApp />
  </React.StrictMode>,
  document.getElementById("root")
);

// when React is loaded 'trouble loading' is not needed
const troubleLoading = document.querySelector(".trouble-loading");
if (troubleLoading && troubleLoading.parentElement) {
  troubleLoading.parentElement.removeChild(troubleLoading);
}
