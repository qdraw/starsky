// @ts-check
// Note: type annotations allow type checking and IDEs autocompletion

const lightCodeTheme = require("prism-react-renderer/themes/github");
const darkCodeTheme = require("prism-react-renderer/themes/dracula");

let url = "http://localhost:3003";
if (process.env.DOCS_URL && process.env.DOCS_URL.startsWith("https://")) {
	url = process.env.DOCS_URL;
}

let baseUrl = "/"
if (process.env.DOCS_BASE_URL && process.env.DOCS_BASE_URL.startsWith("/")) {
	baseUrl = process.env.DOCS_BASE_URL;
}

console.log(`url ${url}`);
console.log(`baseUrl ${baseUrl}`);

/** @type {import('@docusaurus/types').Config} */
const config = {
	title: "Starsky",
	tagline: "Photo-management done right",
	url,
	baseUrl,
	onBrokenLinks: "throw",
	onBrokenMarkdownLinks: "warn",
	favicon: "img/favicon.ico",

	// GitHub pages deployment config.
	// If you aren't using GitHub pages, you don't need these.
	organizationName: "qdraw", // Usually your GitHub org/user name.
	projectName: "starsky", // Usually your repo name.

	// Even if you don't use internalization, you can use this field to set useful
	// metadata like html lang. For example, if your site is Chinese, you may want
	// to replace "en" with "zh-Hans".
	i18n: {
		defaultLocale: "en",
		locales: ["en"],
	},

	presets: [
		[
			"@docusaurus/preset-classic",
			/** @type {import('@docusaurus/preset-classic').Options} */
			({
				docs: {
					sidebarPath: require.resolve("./sidebars.js"),

					// Please change this to your repo.
					// Remove this to remove the "edit this page" links.
				},
				blog: {
					postsPerPage: 5,
					showReadingTime: true
				},
				theme: {
					customCss: require.resolve("./src/css/custom.css"),
				},
				gtag: {
					trackingID: process.env.GTAG ? process.env.GTAG : 'G-999X9XX9XX',
					anonymizeIP: true,
				}
			}),
		],
	],

	themeConfig:
		/** @type {import('@docusaurus/preset-classic').ThemeConfig} */
		({
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
				theme: lightCodeTheme,
				darkTheme: darkCodeTheme,
			},
		}),
};

module.exports = config;
