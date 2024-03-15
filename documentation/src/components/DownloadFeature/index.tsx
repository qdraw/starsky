import clsx from "clsx";
import { ReactElement, useEffect, useState } from "react";
import Button from "../button";
import styles from "./styles.module.css";

type FeatureItem = {
  type: string;
  title?: string;
  src?: any;
  description?: ReactElement;
};

function detectOS() {
  let OSName = "Unknown OS";
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
        {src ? <img src={src} alt={title} /> : null}
      </div>
    </div>
  );
}

export default function DownloadFeatures(): ReactElement {
  const [downloadUrl1, setDownloadUrl1] = useState(
    "https://github.com/qdraw/starsky/releases/latest/"
  );
  const [downloadButton1Text, setDownloadButton1Text] = useState("Download App on Github");
  const [systemDescription, setSystemDescription] = useState("");
  const [downloadButton2Text, setDownloadButton2Text] = useState("");
  const [downloadUrl2, setDownloadUrl2] = useState("");

  useEffect(() => {
    fetch("https://api.github.com/repos/qdraw/starsky/releases").then((result) => {
      console.log(result.ok);
      if (result.ok) {
        result.json().then((json) => {
          const firstItem = json.find((item) => !item.prerelease);

          const windowsItem = firstItem.assets.find(
            (item: { name: string }) => item.name === "starsky-win-x64-desktop.exe"
          );
          const macIntelItem = firstItem.assets.find(
            (item: { name: string }) => item.name === "starsky-mac-x64-desktop.dmg"
          );
          const macArm64Item = firstItem.assets.find(
            (item: { name: string }) => item.name === "starsky-mac-arm64-desktop.dmg"
          );
          if (detectOS() === "Windows") {
            setDownloadUrl1(windowsItem.browser_download_url);
            setDownloadButton1Text("Download App for Windows " + firstItem.tag_name);
          } else if (detectOS() === "Mac") {
            setDownloadUrl1(macIntelItem.browser_download_url);
            setDownloadButton1Text("Download App for Intel Mac " + firstItem.tag_name);
            setSystemDescription(
              "<b>You need to manually sign the code because we haven't purchased certificates from Apple:</b>" +
                "<br />Run this command in the terminal: <br /> " +
                "<code>codesign --force --deep -s - /Applications/Starsky.app && xattr -rd com.apple.quarantine /Applications/Starsky.app</code>" +
                "<br /><br />" +
                "<a href='/docs/getting-started/desktop/desktop-macos'>Read more about how to install on Mac</a> "
            );
            if (macArm64Item?.name) {
              setDownloadUrl2(macArm64Item.browser_download_url);
              setDownloadButton2Text(`Apple Silicon ${firstItem.tag_name}`);
            }
          } else {
            setSystemDescription(
              "You need to manually sign the code because we haven't purchased certificates from Apple<br />" +
                "<a href='/docs/getting-started/desktop/desktop-macos'>Read more about how to install on Mac</a>"
            );
          }
        });
      }
    });
  }, []);

  const FeatureList: FeatureItem[] = [
    {
      type: "col--4",
      title: "Download Desktop App",
      description: (
        <>
          Starsky is a free photo-management tool. It acts as an accelerator to find and organize
          images driven by meta information.
          <br />
          <br />
          <Button href={downloadUrl1} color="#25c2a0">
            {downloadButton1Text}
          </Button>
          {downloadButton2Text ? (
            <Button href={downloadUrl2} color="#25c2a0">
              {downloadButton2Text}
            </Button>
          ) : null}
          {systemDescription ? (
            <>
              <br />
              <br />
              <p dangerouslySetInnerHTML={{ __html: systemDescription }} />
            </>
          ) : null}
          <br />
          <Button
            href={"https://github.com/qdraw/starsky/releases/latest/"}
            color={"rgb(38, 50, 56)"}
          >
            Download other versions
          </Button>
          <br />
          <br />
        </>
      ),
    },
    {
      type: "col--6",
      src: require("@site/static/img/starsky-mac-v050-home-nl.jpg").default,
    },
  ];

  return (
    <section className={styles.features}>
      <div className="container">
        <div className="row">
          {FeatureList.map((props, idx) => (
            <Feature key={props.title} {...props} />
          ))}
        </div>
        <div className="row">
          <i>
            By downloading you agree to{" "}
            <a href="https://docs.qdraw.nl/legal/toc.en.html" data-test="toc">
              Starsky's Conditions of Use.
            </a>{" "}
            Please see our{" "}
            <a href="https://docs.qdraw.nl/legal/privacy-policy.en.html" data-test="privacy">
              Privacy Notice
            </a>{" "}
            and our{" "}
            <a href="https://docs.qdraw.nl/legal/privacy-policy.en.html#cookie">Cookies Notice</a>
          </i>
        </div>
      </div>
    </section>
  );
}
