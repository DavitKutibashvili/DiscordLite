var connectionUserCount = new signalR.HubConnectionBuilder().withUrl("/hubs/userCount").build();

connectionUserCount.on("UpdateTotalViews", (value) => {
    var newCountSpan = document.getElementById("totalViewsCounter");
    newCountSpan.textContent = value;
});
connectionUserCount.on("UpdateTotalUsers", (value) => {
    var newCountSpan = document.getElementById("totalUsersCounter");
    newCountSpan.textContent = value;
});

function newWindowLoadedOnClient() {
    connectionUserCount.invoke("NewWindowLoaded").then((value) => console.log(value));
}

function fulfilled() {
    console.log("Connection to user count hub established.");
    newWindowLoadedOnClient();
}
function rejected() {
    console.log("Connection to user count hub failed.");
}

connectionUserCount.start().then(fulfilled).catch(rejected);