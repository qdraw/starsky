
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


document.querySelector("#file_selector").addEventListener('click', function() {
    window.api.send("settings_default_app", {
        showOpenDialog: true
    });
});

document.querySelector("#file_selector_reset").addEventListener('click', function() {
    window.api.send("settings_default_app", {
        reset: true
    });
});

window.api.receive("settings_default_app", (data) => {
    document.querySelector("#file_selector_result").innerHTML = data;
});

window.api.send("settings_default_app",null);

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

document.querySelector("#settings_update_policy input").addEventListener("click",function(){
    window.api.send("settings_update_policy",this.checked);
})

window.api.receive("settings_update_policy", (data) => {
    document.querySelector("#settings_update_policy input").checked = data;
});

window.api.send("settings_update_policy",null);


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