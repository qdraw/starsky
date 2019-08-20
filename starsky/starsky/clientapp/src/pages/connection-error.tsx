import React from 'react';
import Link from '../components/Link';

function ConnectionError() {
  return (
    <div>
      <h1>Connection Error</h1>
      <p>
        <Link href="/">To home page</Link>
      </p>
    </div>
  );
}

export default ConnectionError;
