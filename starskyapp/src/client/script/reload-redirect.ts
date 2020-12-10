
/**
 * no slash at end
 * @param {*} domainUrl 
 * @param {*} count 
 * @param {*} maxCount 
 */
function warmupScript(domainUrl : string, apiVersion: number, count: number, maxCount: number) {

    var appendAfterDomainUrl = ""
    var rememberUrl = new URLSearchParams(window.location.search).get("remember-url");
    if (rememberUrl) {
      appendAfterDomainUrl = decodeURI(rememberUrl);
    }
  
    fetch(domainUrl + '/api/health')
      .then((response) => {
        if (response.status === 200 || response.status === 503) {
          fetch(domainUrl + '/api/health/version', { method: 'POST',  
          headers: {
            "x-api-version": `${apiVersion}`
          }
        }).then((versionResponse) => {
              if (versionResponse.status === 200) {
                window.location.href = domainUrl + appendAfterDomainUrl;
                // when not navigating check: .on('will-navigate'
                return;
              }
              if (versionResponse.status === 400 && document.querySelectorAll('.upgrade').length === 1) {
                const upgradeElement = document.querySelector('.upgrade') as HTMLElement;
                if (upgradeElement) {
                    upgradeElement.style.display = 'block';
                }
                
                const preloaderElement = document.querySelector('.preloader') as HTMLElement;
                if (preloaderElement) {
                    preloaderElement.style.display = 'none';
                }
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
    (window as any).api.send("settings",null);
  
    (window as any).api.receive("settings", (data : any) => {
      if (!data || !data.remote) {
        document.title += ` going to default`
        warmupScript('http://localhost:9609', data.apiVersion, 0, 300);
        return;
      }
  
      if(data.remote && data.location) {
        document.title += ` going to ${data.location}`
        warmupScript(data.location, data.apiVersion ,0, 300)
      }
    });
  }
  
  warmupLocalOrRemote();
  