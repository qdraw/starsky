
// document.querySelector('#switch_local').addEventListener('click', changeRemoteToggle(true));

document.querySelector('#switch_local').addEventListener('change', function() {
    changeRemoteToggle(false)
});

document.querySelector('#switch_remote').addEventListener('change', function() {
    changeRemoteToggle(true)
});

document.querySelector('#remote_location').addEventListener('change', function() {
    changeRemoteLocation(this.value)
});

// console.log(document.querySelector('#switch_local'));

// document.querySelector('#switch_remote').addEventListener('click', changeRemoteToggle(false));
// document.querySelector('#switch_remote').addEventListener('click', console.log(true));



function changeRemoteToggle(isRemote) {
    console.log(isRemote);
    
    window.api.send("settings", {
        "remote": isRemote,
        "location": document.querySelector("#remote_location").value
    });
}
function changeRemoteLocation(location) {
    if (!location) {
        console.error('wrong location')
        return;
    }
    window.api.send("settings", {
        "remote": document.querySelector("#switch_remote").checked,
        "location": location
    });
}

window.api.receive("settings", (data) => {
    console.log(data);

    if(data && data.remote !== undefined) {
        if (data.remote) {
            document.querySelector("#switch_local").checked = false;
            document.querySelector("#switch_remote").checked = true;
            document.querySelector("#remote_location").disabled = false;
        }
        else {
            document.querySelector("#switch_local").checked = true;
            document.querySelector("#switch_remote").checked = false;
            document.querySelector("#remote_location").disabled = true;
        }

        document.querySelector("#remote_location").value = data.location;
    }
    if(data && data.locationOk !== undefined) {
        document.querySelector("#locationOk").innerHTML = data.locationOk ? "OK" : "FAIL";
    }
});

window.api.send("settings",null);