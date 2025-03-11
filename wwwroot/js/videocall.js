let peerConnection;
let connection = null;
let a = null;
const config = {
    iceServers: [{ urls: 'stun:stun.l.google.com:19302' }]
};

function startVideoCall() {
    peerConnection = new RTCPeerConnection(config);
    navigator.mediaDevices.getUserMedia({ video: true, audio: true }).then(localStream => {
        document.getElementById('localVideo').srcObject = localStream;

        peerConnection.ontrack = (event) => {
            console.log("🔥 Track event received!", event);

            if (event.streams.length > 0) {
                let remoteStream = document.getElementById('remoteVideo').srcObject;

                if (!remoteStream) {
                    remoteStream = new MediaStream();
                    document.getElementById('remoteVideo').srcObject = remoteStream;
                    console.log("🎥 Remote stream initialized");
                }

                event.streams[0].getTracks().forEach(track => {
                    if (!remoteStream.getTracks().includes(track)) {
                        remoteStream.addTrack(track);
                        console.log(`✅ Added remote track: ${track.kind}`);
                    }
                });
            } else {
                console.warn("⚠️ No streams received!");
            }
        };


        localStream.getTracks().forEach(track => {
            console.log("Adding track:", track.kind);
            peerConnection.addTrack(track, localStream);
        });

        // Thiết lập sự kiện onicecandidate để gửi các ICE candidates đến peer khác
        peerConnection.onicecandidate = (event) => {
            if (event.candidate) {
                console.log("Sending ICE candidate", event.candidate);
                sendToServer({
                    type: 'candidate',
                    candidate: event.candidate,
                });
            }
        };

        peerConnection.createOffer().then(offer => {
            return peerConnection.setLocalDescription(offer);
        }).then(() => {
            sendToServer({
                type: 'offer',
                offer: peerConnection.localDescription,
            });
        }).catch(error => {
            console.error("Error creating offer: ", error);
        });
    }).catch(error => {
        console.error("Error accessing media devices: ", error);
    });
}
function sendToServer(message) {

    if (!connection || connection.state !== "Connected") {
       
            setTimeout(() => sendToServer(message), 500);
        
        return;
    }
   
    connection.invoke("SendVideoCall",id, JSON.stringify(message)).catch(console.error);
}

function initializeSignalR() {


    connection = new signalR.HubConnectionBuilder()
        .withUrl("/conversionHub")
        .withAutomaticReconnect()
        .build();

    connection.start().then(() => {
        console.log("SignalR connected");
        connection.invoke("JoinConversionGroup", id).catch(console.error);
    }).catch(console.error);

    connection.on("ReceiveCallVideo", (message) => {
        
        const parsedMessage = JSON.parse(message);
        if (parsedMessage.type === 'accept') {
            startVideoCall();

        }

        if (parsedMessage.type === 'offer') {
            a = parsedMessage
            console.log(1)
            console.log(a)
            peerConnection.setRemoteDescription(new RTCSessionDescription(parsedMessage.offer))
                .then(() => peerConnection.createAnswer())
                .then(answer => peerConnection.setLocalDescription(answer))
                .then(() => sendToServer({
                    type: 'answer',
                    answer: peerConnection.localDescription,
                }))
                .catch(error => {
                    console.error("Error handling offer: ", error);
                });

        } else if (parsedMessage.type === 'answer') {
            console.log("🟢 Received ANSWER");
            peerConnection.setRemoteDescription(new RTCSessionDescription(parsedMessage.answer))
                .catch(error => {
                    console.error("Error setting remote description from answer: ", error);
                });

        } else if (parsedMessage.type === 'candidate') {
            console.log("🔵 Received CANDIDATE");
            
            console.log(parsedMessage)
            peerConnection.addIceCandidate(new RTCIceCandidate(parsedMessage.candidate))
                .catch(error => {
                    console.error("Error adding ICE candidate: ", error);
                });
        }
    });
    window.addEventListener('beforeunload', function () {
        connection.stop().then(() => console.log("SignalR disconnected")).catch(console.error);
    });
}
function endVideoCall() {
    if (peerConnection) {
        // Dừng các track của localStream
        const localVideo = document.getElementById('localVideo');
        const remoteVideo = document.getElementById('remoteVideo');

        if (localVideo.srcObject) {
            localVideo.srcObject.getTracks().forEach(track => track.stop());
            document.getElementById('localVideo').style.width = "100px";
            document.getElementById('localVideo').style.height = "100px";

            
        }

        if (remoteVideo.srcObject) {
            remoteVideo.srcObject.getTracks().forEach(track => track.stop());
            document.getElementById('remoteVideo').style.width = "100px";
            document.getElementById('remoteVideo').style.height = "100px";
        }

        peerConnection.close();
        peerConnection = null;
        callStarted = false;
        localVideo.srcObject = null;
        remoteVideo.srcObject = null;
        console.log("Call ended");
        sendToServer({ type: 'endCall' });
    }
}
document.addEventListener('DOMContentLoaded', function () {
    initializeSignalR();
    if (isSend) {
        startVideoCall();
    }
    


    document.getElementById('accept-call').addEventListener('click', function () {
        startVideoCall();

    });
    

});
