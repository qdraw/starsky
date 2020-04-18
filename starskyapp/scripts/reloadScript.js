

function warmupScript(count, maxCount) {
  fetch('http://localhost:5000/api/health')
    .then((response) => {
      if (response.status === 200 || response.status === 503) {
        window.location.href = 'http://localhost:5000';
      }
    }).catch((error)=>{
      console.log(error);
      if (count <= maxCount) {
        count++
        setTimeout(() => {
          warmupScript(count, maxCount)
        }, 50);
      }
      else {
        alert("application failed to start")
      }
    });
}

warmupScript(0, 100)
