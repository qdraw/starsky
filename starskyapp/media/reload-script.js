

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
        fetch(domainUrl + '/api/health/version', {headers: {"X-API-Version": apiVersion}})
          .then((response2) => {
            if (status === 200) {
              window.location.href = domainUrl;
            }
            if (status === 400) {
              window.location.href = "upgrade.html";
            }
            
            alert("application failed to start");

          }).catch((error) => {
            alert("application failed to start");
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
        alert("application failed to start")
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
