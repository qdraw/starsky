import { Link } from '@reach/router';
import React from 'react';

function ConnectionError() {
  return (
    <div>
      <h1>Connection Error</h1>
      <p>
        <Link to="/">To home page</Link>
      </p>
    </div>
  );
}

export default ConnectionError;
