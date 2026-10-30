import React from "react";
import Layout from "@theme/Layout";
import Head from "@docusaurus/Head";

// the legal pages reference this page /starsky

export default function Starsky() {
	return (
		<Layout title="Starsky" description="No content page">
			<Head>
				<meta name="robots" content="noindex" />
			</Head>
			<br />
			<a href="/">Click here to go to the home page</a>
		</Layout>
	);
}
