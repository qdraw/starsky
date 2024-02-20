import CurrentLocationButton from "./current-location-button";

export default {
  title: "components/atoms/current-location-button"
};

export const Default = () => {
  return (
    <CurrentLocationButton
      callback={(result) => {
        alert(`${result.latitude} ${result.longitude}`);
      }}
    ></CurrentLocationButton>
  );
};

Default.storyName = "default";
