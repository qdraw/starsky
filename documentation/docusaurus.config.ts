import type * as Preset from "@docusaurus/preset-classic";
import type { Config } from "@docusaurus/types";
import { themes as prismThemes } from "prism-react-renderer";

let url = "http://localhost:3003";
if (process.env.DOCS_URL && process.env.DOCS_URL.startsWith("https://")) {
  url = process.env.DOCS_URL;
}

let baseUrl = "/";
if (process.env.DOCS_BASE_URL && process.env.DOCS_BASE_URL.startsWith("/")) {
  baseUrl = process.env.DOCS_BASE_URL;
}

console.log(`url ${url}`);
console.log(`baseUrl ${baseUrl}`);

const scripts = {} as any;
if (process.env.GTAG) {
  console.log("gtag enabled");
  scripts.scripts = [
    `https://www.googletagmanager.com/gtag/js?id=${process.env.GTAG}`,
    "/analytics.js",
  ];
}

const config: Config = {
  title: "Starsky",
  tagline: "Photo-management done right",
  favicon: "img/favicon.ico",

  // Set the production url of your site here
  url,
  // Set the /<baseUrl>/ pathname under which your site is served
  // For GitHub pages deployment, it is often '/<projectName>/'
  baseUrl,

  // GitHub pages deployment config.
  // If you aren't using GitHub pages, you don't need these.
  organizationName: "qdraw", // Usually your GitHub org/user name.
  projectName: "starsky", // Usually your repo name.

  onBrokenLinks: "throw",
  onBrokenMarkdownLinks: "warn",

  // Even if you don't use internationalization, you can use this field to set
  // useful metadata like html lang. For example, if your site is Chinese, you
  // may want to replace "en" with "zh-Hans".
  i18n: {
    defaultLocale: "en",
    locales: ["en"],
  },

  presets: [
    [
      "classic",
      {
        docs: {
          sidebarPath: "./sidebars.ts",
          // Please change this to your repo.
          // Remove this to remove the "edit this page" links.
          //   editUrl:
          //     'https://github.com/facebook/docusaurus/tree/main/packages/create-docusaurus/templates/shared/',
        },
        blog: {
          showReadingTime: true,
          // Please change this to your repo.
          // Remove this to remove the "edit this page" links.
          //   editUrl:
          //     'https://github.com/facebook/docusaurus/tree/main/packages/create-docusaurus/templates/shared/',
        },
        theme: {
          customCss: "./src/css/custom.css",
        },
      } satisfies Preset.Options,
    ],
  ],
  ...scripts,
  plugins: [
    [
      require.resolve("@cmfcmf/docusaurus-search-local"),
      {
        indexDocs: true,
        indexDocSidebarParentCategories: 3,
        indexPages: true,
        indexBlog: true,
        maxSearchResults: 8,
      },
    ],
  ],

  themeConfig: {
    navbar: {
      title: "Starsky",
      logo: {
        alt: "Starsky Logo",
        src: "img/detective.png",
      },
      items: [
        {
          type: "doc",
          docId: "getting-started/readme",
          position: "left",
          label: "Getting started",
        },
        {
          type: "doc",
          docId: "features/readme",
          position: "left",
          label: "Features",
        },
        {
          type: "doc",
          docId: "advanced-options/readme",
          position: "left",
          label: "Advanced options",
        },
        {
          type: "doc",
          docId: "developer-guide/readme",
          position: "left",
          label: "Developer Guide",
        },
        {
          label: "Download",
          href: "/download",
        },
        {
          href: "https://github.com/qdraw/starsky",
          label: "GitHub",
          position: "right",
        },
      ],
    },
    footer: {
      style: "dark",
      links: [
        {
          title: "Docs",
          items: [
            {
              label: "Getting started",
              to: "/docs/getting-started",
            },
            {
              label: "All features",
              to: "/docs/features",
            },
            {
              label: "Application Blog",
              to: "/blog",
            },
          ],
        },
        {
          title: "Community",
          items: [
            {
              label: "Discussions",
              href: "https://github.com/qdraw/starsky/discussions",
            },
          ],
        },
        {
          title: "More",
          items: [
            {
              label: "Qdraw Blog (in Dutch)",
              to: "https://qdraw.nl/blog/",
            },
            {
              label: "GitHub",
              href: "https://github.com/qdraw/starsky",
            },
          ],
        },
      ],
      copyright: `Copyright &copy; ${new Date().getFullYear()} Starsky`,
    },
    prism: {
      theme: prismThemes.github,
      darkTheme: prismThemes.dracula,
    },
  } satisfies Preset.ThemeConfig,
};

export default config;
