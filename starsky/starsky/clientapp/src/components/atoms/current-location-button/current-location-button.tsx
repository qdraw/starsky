import React, { useState } from 'react';

export type CurrentLocationButtonPropTypes = {
  callback?(coords: Coordinates): void;
}

const CurrentLocationButton: React.FunctionComponent<CurrentLocationButtonPropTypes> = ({ callback }) => {

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
    navigator.geolocation.getCurrentPosition(currentPositionSuccess, currentPositionError);
  }

  return (
    <button data-test="location_on" className={!error ? "current-location icon icon--location_on" : "current-location icon icon--wrong_location"}
      onClick={locationOn}>Location</button>
  );
};

export default CurrentLocationButton
