

/**
 * no slash at end
 * @param {*} domainUrl 
 * @param {*} count 
 * @param {*} maxCount 
 */
function warmupScript(domainUrl, count, maxCount) {
  fetch(domainUrl + '/api/health')
    .then((response) => {
      if (response.status === 200 || response.status === 503) {
        window.location.href = domainUrl;
      }
    }).catch((error) => {
      console.log('error', error);
      if (count <= maxCount) {
        count++
        setTimeout(() => {
          warmupScript(domainUrl, count, maxCount)
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
      warmupScript('http://localhost:9609', 0, 300)
      return;
    }

    if(data.remote && data.location) {
      console.log("d",data.location);
      
      warmupScript(data.location,0, 300)
    }
  });
}

warmupLocalOrRemote();
