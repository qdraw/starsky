

/**
 * no slash at end
 * @param {*} domainUrl 
 * @param {*} count 
 * @param {*} maxCount 
 */
function warmupScript(domainUrl, apiVersion, count, maxCount) {
  fetch(domainUrl + '/api/health')
    .then((response) => {
      if (response.status === 200 || response.status === 503) {
        fetch(domainUrl + '/api/health/version', { method: 'POST',  headers: {"X-API-Version": apiVersion}})
          .then((versionResponse) => {
            if (versionResponse.status === 200) {
              window.location.href = domainUrl;
              return;
            }
            if (versionResponse.status === 400) {
              window.location.href = "upgrade.html";
              return;
            }
            alert(`#${versionResponse.status} - Version check failed, please try to restart the application`);
            
          }).catch((error) => {
            alert("no connection to version check, please restart the application");
          });
      }
    }).catch((error) => {
      console.log('error', error);
      if (count <= maxCount) {
        count++
        setTimeout(() => {
          warmupScript(domainUrl, apiVersion, count, maxCount)
        }, 200);
      }
      else {
        alert("no connection to the internal component, please restart the application")
      }
    });
}

function warmupLocalOrRemote() {
  window.api.send("settings",null);

  window.api.receive("settings", (data) => {
    console.log(data);

    if (!data || !data.remote) {
      console.log('default');
      warmupScript('http://localhost:9609',data.apiVersion, 0, 300)
      return;
    }

    if(data.remote && data.location) {
      console.log("d",data.location);
      
      warmupScript(data.location, data.apiVersion ,0, 300)
    }
  });
}

warmupLocalOrRemote();
