import { Link } from '@reach/router';
import React from 'react';


function ErrorPage() {
  return (
    <div>
      <h1>Error</h1>
      <p>
        <Link to="/">To home page</Link>
      </p>
    </div>
  );
}

export default ErrorPage;
