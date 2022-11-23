import React from "react";
import Layout from "@theme/Layout";
import DownloadFeatures from "../components/DownloadFeature";

// https://qdraw.github.io/starsky/assets/download/download.html
export default function Hello() {
	return (
		<Layout title="Hello" description="Hello React Page">

      <DownloadFeatures />

		</Layout>
	);
}
