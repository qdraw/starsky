
var downloadUrl = document.getElementById("js-settings").getAttribute("data-downloadUrl");

var gpx = downloadUrl; // URL to your GPX file or the GPX itself



if (document.querySelectorAll(".main-image").length === 1) {
    document.querySelector(".main-image").innerHTML = "<div id=\"map\">" +
        "<div class=\"lock-map\">" +
        "<a class=\"btn btn-info js-enableMap\">" +
        "Zet kaartnavigeer opties aan</a></div><div class=\"info\"></div></div>";
    
    document.querySelector(".lock-map .js-enableMap")
        .addEventListener("click",
            function () {
                enableMap()
            }, false);
}

var map = L.map('map',{ });

function disableMap() {
    document.querySelector(".main-image #map").classList.add("disabled");
    map._handlers.forEach(function(handler) {
        handler.disable();
    });
    map.removeControl( map.zoomControl );
}
function enableMap() {
    document.querySelector(".main-image #map").classList.remove("disabled");
    map._handlers.forEach(function(handler) {
        handler.enable();
    });
    map.addControl( map.zoomControl );
}

disableMap();

L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
}).addTo(map);



var blueIcon = new L.icon({
    iconUrl: '../images/fa-map-marker-blue.svg',
    shadowUrl: '../images/marker-shadow.png',
    iconSize:     [50, 50], // size of the icon
    shadowSize:   [50, 50], // size of the shadow
    iconAnchor:   [25, 50], // point of the icon which will correspond to marker's location
    shadowAnchor: [15, 55],  // the same for the shadow
    popupAnchor:  [0, -50] // point from which the popup should open relative to the iconAnchor
});

var gpx = gpx.replace("&amp;", "&");
new L.GPX(gpx,
    {
        async: true,
        marker_options: {
            startIcon: blueIcon,
            endIcon: blueIcon
        },
        polyline_options: { // the line
            color: "#000"
        }

    }).on('loaded', function(e) {
    map.fitBounds(e.target.getBounds());

    document.querySelector(".main-image #map .info").innerHTML =
        '<div class="get_distance"></div> <div class="get_total_speed"></div> <div class="get_moving_speed"></div>';

    var distance = (e.target.get_distance() / 1000).toFixed(2);
    document.querySelector(".main-image #map .get_distance").innerHTML = "Afstand " + distance + " km";
    document.querySelector(".main-image #map .get_total_speed").innerHTML = "Gemiddelde snelheid " + e.target.get_total_speed().toFixed(2) + " km/h";

    console.log(e.target.get_distance());
}).addTo(map);