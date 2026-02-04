---
sidebar_position: 2
---

# React

ReactJS is a declarative, efficient, and flexible JavaScript library for building reusable UI components.
It is an open-source, component-based front end library responsible only for the view layer of the application.

In this project we use ReactJS to build the user interface of the application.
The project is based on the Vite with Typescript template.

## Client-side rendering

React is only used as client-side rendering. For the server that has low CPU specs this performs better. This chosen to make the application easier and faster on low resourceful servers like a Raspberry Pi

## Vite

Vite is a build tool and development server that is designed to enhance the development workflow for JavaScript applications. It offers fast, efficient, and near-instantaneous hot module replacement, making it an excellent choice for modern web development, especially in the context of Vue.js and React applications. Vite leverages native ES modules to improve build times and development speed, making it a popular choice among developers looking for a swift and productive development experience.

## TypeScript

TypeScript is a syntactic superset of JavaScript which adds static typing.
This basically means that TypeScript adds syntax on top of JavaScript, allowing developers to add types.

It adds optional static typing to JavaScript,
making it easier to catch type-related errors during development and to write large-scale applications.
The TypeScript code is transpiled to JavaScript, allowing it to run on any platform that supports JavaScript.
TypeScript is widely used for developing large and complex web applications,
and its type system provides better documentation and improved code navigation.
TypeScript is supported by a large and growing community,
and many popular libraries and frameworks have TypeScript definitions available,
making it easier to use these tools with TypeScript.

## Jest unit tests

Jest is a popular JavaScript testing framework that provides a seamless testing experience with its simple APIs,
built-in mocking, and assertion libraries.
It works with any JavaScript codebase and can be easily integrated into an existing project.
Jest also provides features like watch mode, code coverage reports, and parallel testing.
It is widely used for testing React applications but can be used for testing any JavaScript code.

In this project we use Jest to test the typescript code and the React components of the application.

We currently don't use snapshot testing.

> We aim to achieve a code line unit test coverage of more than 90%

## React Testing Library

React Testing Library is a popular testing library for React applications.
It is designed to help developers test React components in a way that is similar to
how users interact with the components in a real application.
It focuses on testing the behavior of a component, rather than its implementation details.
React Testing Library provides APIs for performing common testing tasks, like rendering components,
querying elements, and simulating user events. By using this library,
developers can write tests that are more focused on the user experience and less on implementation details,
making their tests more maintainable and less prone to break with future changes.
