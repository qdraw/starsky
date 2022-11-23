import clsx from "clsx";
import React from "react";
import styles from "./styles.module.css";

type FeatureItem = {
	title: string;
	Svg: React.ComponentType<React.ComponentProps<"svg">>;
	description: JSX.Element;
};

const FeatureList: FeatureItem[] = [
	{
		title: "Easy to Use",
		Svg: null,
		description: (
			<>Browse all your photos and videos, duplicates or video formats</>
		),
	},
	{
		title: "100% Privacy 🔒",
		Svg: null,
		// Svg: require('@site/static/img/undraw_docusaurus_tree.svg').default,
		description: (
			<>
				Your data will never be shared with Google, Amazon, Microsoft or Apple
				unless you intentionally upload files to one of their services.
			</>
		),
	},
	{
		title: "Search and find",
		Svg: null,
		// Svg: require('@site/static/img/undraw_docusaurus_react.svg').default,
		description: (
			<>Easily find specific pictures using powerful search filters</>
		),
	},
];

function Feature({ title, Svg, description }: FeatureItem) {
	return (
		<div className={clsx("col col--4")}>
			<div className="text--center">
				{/* <Svg className={styles.featureSvg} role="img" /> */}
			</div>
			<div className="text--center padding-horiz--md">
				<h3>{title}</h3>
				<p>{description}</p>
			</div>
		</div>
	);
}

export default function HomepageFeatures(): JSX.Element {
	return (
		<section className={styles.features}>
			<div className="container">
				<div className="row">
					{FeatureList.map((props, idx) => (
						<Feature key={idx} {...props} />
					))}
				</div>
			</div>
		</section>
	);
}
