import React, { useState } from "react";
import { Coordinates, Position } from "../../../shared/coordinates-position.types";

export type CurrentLocationButtonPropTypes = {
  callback?(coords: Coordinates): void;
};

const CurrentLocationButton: React.FunctionComponent<CurrentLocationButtonPropTypes> = ({
  callback
}) => {
  function currentPositionSuccess(position: Position) {
    setError(false);
    if (!callback) return;
    callback(position.coords);
  }

  const [error, setError] = useState(false);

  function currentPositionError() {
    setError(true);
  }

  function locationOn() {
    if (!navigator.geolocation) {
      setError(true);
      return;
    }
    // typescript:S5604 - is used to get the current position of the device
    navigator.geolocation.getCurrentPosition(currentPositionSuccess, currentPositionError);
  }

  return (
    <button
      data-test="current-location-button"
      className={
        !error
          ? "current-location icon icon--location_on"
          : "current-location icon icon--wrong_location"
      }
      onClick={locationOn}
    >
      Location
    </button>
  );
};

export default CurrentLocationButton;
