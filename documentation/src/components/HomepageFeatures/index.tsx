import Link from "@docusaurus/Link";
import clsx from "clsx";
import React from "react";
import styles from "./styles.module.css";

type FeatureItem = {
  title: string;
  ImageUrl?: string;
  description: JSX.Element;
};

const FeatureListRow1: FeatureItem[] = [
  {
    title: "Quick search, quick find",
    ImageUrl: "https://media.qdraw.nl/log/de-7-dingen-die-ik-miste-bij-het-beheren-van-mijn-foto-collectie/embeded/01_video_search_cloud_starsky_v050.gif",
    description: (
      <>Finding that one photo you're looking for can be quite difficult.
        That's why finding what you've been looking for quickly and easily is a great way to get a
        lot more done in a shorter amount of time.
        When you place a number of keywords in the meta data field in an image,
        you can search for them later. But also by date, description and information
        that is stored in the camera. So quick search, quick find.
        &nbsp;<Link href="docs/features/search">Read more about search</Link>&nbsp;

      </>
    ),
  },
  {
    title: "100% Compatable and privacy by design 🔒",
    ImageUrl: "https://media.qdraw.nl/log/de-7-dingen-die-ik-miste-bij-het-beheren-van-mijn-foto-collectie/1000/07_exiftool_starsky_v042_kl1k.jpg",
    description: (
      <>
        All information about a specific photo that you enter into the photo management tool will
        end up in the photo.
        That includes labels, description and location information.
        The tool uses standards, all meta information contained in the image is also written into
        the image itself.
        So you can switch at any time to another photo management tool.
        &nbsp;<Link href="docs/features/metadata">Read more about meta data usage</Link>&nbsp;
      </>
    ),
  },
  {
    title: "Smart addition of geolocation data",
    ImageUrl: "https://media.qdraw.nl/log/de-7-dingen-die-ik-miste-bij-het-beheren-van-mijn-foto-collectie/embeded/04_video_starsky_v052.gif",
    description: (
      <>
        Mobile phones accurately track the location of a photo. However, many cameras don't do this
        yet.
        To avoid having to manually add location data to each photo, I use a sports app on my phone.
        It keeps track of my location. I then export this information from the sports app, after
        which the photo management application automatically brings it all together.
        Then I put the gpx file containing all the locations and times in the same folder and run a
        script that automates this integration.
      </>
    ),
  },
];

const FeatureListRow2: FeatureItem[] = [
  {
    title: "Without connection to the internet",
    ImageUrl: "https://media.qdraw.nl/log/de-7-dingen-die-ik-miste-bij-het-beheren-van-mijn-foto-collectie/1000/06_20200516_144415_d_e_starsky_v040_kl1k.jpg",
    description: (
      <>In some places the connection is not yet that good to send gigabytes of information over the
        line. A cloud service is not the most convenient solution at such a time.
        &nbsp;<Link href="docs/getting-started/raspberry-pi">Read more about running on a Raspberry
          Pi</Link>
      </>
    ),
  },
  {
    title: "Import photos and videos with minimal manual actions",
    ImageUrl: "https://media.qdraw.nl/log/de-7-dingen-die-ik-miste-bij-het-beheren-van-mijn-foto-collectie/embeded/05_video_starsky_v052.gif",
    description: (
      <>
        Transferring the photos and videos from your camera to your photo library is not
        complicated, but often a manual task.
        Yet this is easy to automate. You connect your SD card or USB cable and the computer does
        the rest of the process independently.
        If your photo library has more structure, for example by organizing the photos by date, you
        can also configure this in the settings.
        &nbsp;<Link href="docs/features/import">Read more about importing</Link>
      </>
    ),
  },

  {
    title: "Prepare for publication and add a watermark",
    ImageUrl: "https://media.qdraw.nl/log/de-7-dingen-die-ik-miste-bij-het-beheren-van-mijn-foto-collectie/embeded/03_video_starsky_v050.gif",
    description: (
      <>
        Before making a blog or social media post, we first make a selection of the photos we have
        taken.
        But how do you get them in the right format? That is often a task that you have to wait
        until you get home and need a computer with the right software.
        Based on these selections I do an automatic publication action. This places a watermark in
        the images.  &nbsp;
        <Link href="docs/features/webhtmlpublish">Read more about webhtmlpublish</Link>
      </>
    ),
  },
];

function Feature({title, ImageUrl, description}: FeatureItem) {
  return (
    <div className={clsx("col col--4")}>

      <div className="text--center padding-horiz--md">
        <h3>{title}</h3>
        <p>{description}</p>
      </div>
      <div className="text--center">
        {ImageUrl && <img src={ImageUrl} alt={title}/>}
      </div>
    </div>
  );
}

export default function HomepageFeatures(): JSX.Element {
  return (
    <section className={styles.features}>
      <div className="container">
        <div className="row">
          {FeatureListRow1.map((props, idx) => (
            <Feature key={props.ImageUrl + props.title + props.description} {...props} />
          ))}
        </div>
        <br/><br/>
        <div className="row">
          {FeatureListRow2.map((props, idx) => (
            <Feature key={props.ImageUrl + props.title + props.description} {...props} />
          ))}
        </div>
      </div>
    </section>
  );
}
