import clsx from "clsx";
import React, { useEffect, useState } from "react";
import Button from "../button";
import styles from "./styles.module.css";

type FeatureItem = {
	type: string;
	title?: string;
	src?: any;
	description?: JSX.Element;
};

function detectOS() {
	var OSName = "Unknown OS";
	if (navigator.userAgent.indexOf("Win") != -1) OSName = "Windows";
	if (navigator.userAgent.indexOf("Mac") != -1) OSName = "Mac";
	if (navigator.userAgent.indexOf("Linux") != -1) OSName = "Linux";
	if (navigator.userAgent.indexOf("Android") != -1) OSName = "Android";
	if (navigator.userAgent.indexOf("like Mac") != -1) OSName = "iOS";
	return OSName;
}

function Feature({ title, type, src, description }: FeatureItem) {
	return (
		<div className={clsx("col " + type)}>
			<div className="text--center padding-horiz--md">
				{title ? <h1>{title}</h1> : null}
				{description ? <p>{description}</p> : null}
				{src ? <img src={src} /> : null}
			</div>
		</div>
	);
}

export default function DownloadFeatures(): JSX.Element {
	const [downloadUrl, setDownloadUrl] = useState("https://github.com/qdraw/starsky/releases/latest/");
	const [downloadButtonText, setDownloadButtonText] = useState("Download App on Github");
	const [systemDiscription, setSystemDiscription] = useState("");

	useEffect(() => {
		fetch("https://api.github.com/repos/qdraw/starsky/releases").then(
			(result) => {
				console.log(result.ok);
				if (result.ok) {
					result.json().then((json) => {
						const firstItem = json.find((item) => !item.prerelease);

						const windowsItem = firstItem.assets.find(
							(item: { name: string }) =>
								item.name === "starsky-win-x64-desktop.exe"
						);
						const macItem = firstItem.assets.find(
							(item: { name: string }) =>
								item.name === "starsky-mac-x64-desktop.dmg"
						);

						if (detectOS() === "Windows") {
							setDownloadUrl(windowsItem.browser_download_url);
							setDownloadButtonText(
								"Download App for Windows " + firstItem.tag_name
							);
						} else if (detectOS() === "Mac") {
							setDownloadUrl(macItem.browser_download_url);
							setDownloadButtonText(
								"Download App for Mac " + firstItem.tag_name
							);
							setSystemDiscription(
								"Mac OS users are warned since we don't have certificates from Apple"
							);
						} else {
							setSystemDiscription(
								"Mac OS users are warned since we don't have certificates from Apple"
							);
						}
					});
				}
			}
		);
	}, []);

	const FeatureList: FeatureItem[] = [
		{
			type: "col--4",
			title: "Download Desktop App",
			description: (
				<>
					Starsky is a free photo-management tool. It acts as an accelerator to
					find and organize images driven by meta information.
					<br />
					<br />
					<Button href={downloadUrl} color="#25c2a0">
						{downloadButtonText}
					</Button>
					{systemDiscription ? (
						<>
							<br />
							<br />
							<b>{systemDiscription}</b>
						</>
					) : null}
					<br />
					<br />
					<a href="https://github.com/qdraw/starsky/releases/latest/">
						Download other versions
					</a>
					<br />
					<br />

				</>
			),
		},
		{
			type: "col--6",
			src: require("@site/static/img/starsky-mac-v043-home-nl.jpg").default,
		},

	];

	return (
		<section className={styles.features}>
			<div className="container">
				<div className="row">
					{FeatureList.map((props, idx) => (
						<Feature key={idx} {...props} />
					))}
				</div>
                <div className="row">
                    <i>
                By downloading you agree to{" "}
						<a
							href="https://docs.qdraw.nl/legal/toc.en.html"
							data-test="toc"
						>
							Starsky's Conditions of Use.
						</a>
						{" "}Please see our{" "}
						<a
							href="https://docs.qdraw.nl/legal/privacy-policy.en.html"
							data-test="privacy"
						>
							Privacy Notice
						</a>{" "}
						and our{" "}
						<a href="https://docs.qdraw.nl/legal/privacy-policy.en.html#cookie">
							Cookies Notice
						</a>
					</i>
                </div>
			</div>
		</section>
	);
}
