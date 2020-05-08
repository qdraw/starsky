import { configure } from "@storybook/react";
import '../src/style/css/00-index.css';

const req = require.context("../src", true, /\.stories\.tsx$/);

function loadStories() {
    req.keys().forEach(req);
}
configure(loadStories, module);
