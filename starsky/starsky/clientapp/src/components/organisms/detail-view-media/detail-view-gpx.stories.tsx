import useLocation from "../../../hooks/use-location/use-location";
import DetailViewGpx from "./detail-view-gpx";

export default {
  title: "components/organisms/detail-view-gpx"
};

export const Default = () => {
  const location = useLocation();

  location.navigate("/?f=/test.gpx");
  return (
    <div className="detailview">
      <DetailViewGpx></DetailViewGpx>
    </div>
  );
};

Default.storyName = "default";
