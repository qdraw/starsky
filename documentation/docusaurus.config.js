// @ts-check
// Note: type annotations allow type checking and IDEs autocompletion

const lightCodeTheme = require("prism-react-renderer/themes/github");
const darkCodeTheme = require("prism-react-renderer/themes/dracula");

/** @type {import('@docusaurus/types').Config} */
const config = {
	title: "Starsky",
	tagline: "Self-hosted photo-management done right",
	url: "https://docs.qdraw.eu",
	baseUrl: "/",
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
				blog: false,
				theme: {
					customCss: require.resolve("./src/css/custom.css"),
				},
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
						docId: "advanced-options/readme",
						position: "left",
						label: "Advanced options",
					},
					{
						type: "doc",
						docId: "api/readme",
						position: "left",
						label: "API",
					},
					{
						label: 'Download',
						href: '/download',
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
								label: "Blog",
								to: "https://qdraw.nl/blog/",
							},
							{
								label: "GitHub",
								href: "https://github.com/qdraw/starsky",
							},
						],
					},
				],
				copyright: `Copyright © ${new Date().getFullYear()} Starsky`,
			},
			prism: {
				theme: lightCodeTheme,
				darkTheme: darkCodeTheme,
			},
		}),
};

module.exports = config;
