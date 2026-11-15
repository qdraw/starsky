import React from "react";
import { ImageFormat } from "../../../interfaces/IFileIndexItem";
import DetailViewMp4 from "./detail-view-mp4";

/**
 * Video (mp4) or audio-only (m4a) player, nothing for other formats
 */
const DetailViewPlayer: React.FunctionComponent<{ imageFormat?: ImageFormat }> = ({
  imageFormat
}) => {
  if (imageFormat === ImageFormat.mp4) return <DetailViewMp4 />;
  if (imageFormat === ImageFormat.m4a) return <DetailViewMp4 audioOnly={true} />;
  return null;
};

export default DetailViewPlayer;
