// # Why does this file exist?
// to import images as url to support the strict CSP

// # How to use it:
// index.d.ts: declare module '*.png'; (no need for the content or value fun)
// index.tsx: /// <reference path='./index.d.ts'/> (props to @redredredredredred)
// index.tsx: import logo from "./logo.png"; (no need for *)
//   <img src={ logo } /> or <div style={``background: url(${logo});``}>…</div > (dunno how to escape backticks in inline code)
// source: https://github.com/microsoft/TypeScript-React-Starter/issues/12#issuecomment-385165319

declare module '*.gif';
