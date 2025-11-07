(() => {
  if (window.fetchIntercepted) return;
  window.fetchIntercepted = true;

  const isMac = !!window.webkit?.messageHandlers?.native;
  const isWindows = !!window.chrome?.webview;

  window.originalFetch = window.fetch;

  window.fetch = async function(resource, options = {}) {
    if (typeof resource === 'string' && resource.startsWith('/api/')) {
      const request = {
        type: 'api',
        url: resource,
        method: options.method || 'GET',
        headers: options.headers || {},
        body: options.body || null
      };

      if (isWindows) {
        const result = await window.chrome.webview.postMessage(JSON.stringify(request));
        return new Response(JSON.stringify(result), {
          status: 200,
          headers: { 'Content-Type': 'application/json' }
        });
      } else if (isMac) {
        window.webkit.messageHandlers.native.postMessage(JSON.stringify(request));
        return new Promise(resolve => {
          window.onNativeResponse = result => {
            resolve(new Response(JSON.stringify(result), {
              status: 200,
              headers: { 'Content-Type': 'application/json' }
            }));
          };
        });
      }
    }
    return window.originalFetch(resource, options);
  };
})();
