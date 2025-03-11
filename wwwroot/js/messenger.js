
let pageIndex = 1;
const pageSize = 4;
let pageIndexMess = 2;
const pageSizeMess = 10;
let isLoading = false;
let connection = null;
const url = window.location.pathname;
const parts = url.split('/');
const id = parts[parts.length - 1];
//
let timeout = null;
function initializeSignalR() {
    if (id === "Messenger" || connection) return;

    connection = new signalR.HubConnectionBuilder()
        .withUrl("/conversionHub")
        .withAutomaticReconnect()
        .build();

    connection.start().then(() => {
        console.log("SignalR connected");
        connection.invoke("JoinConversionGroup", parseInt(id, 10)).catch(console.error);
    }).catch(console.error);

    connection.on("InvitationCallVideo", function (message) {
    
        window.open(`https://1c76-1-54-8-29.ngrok-free.app/messenger/VideoCall?conversionId=${id}&isSend=false&sender=${message}&received=${currentUserName}`, '_blank', "noopener,noreferrer,width=800,height=600");


    });

    connection.on("ReceiveMessenger", function (message) {
        if (message.sender.userName === currentUserName) return;
        const content = `
            <div class="pb-3 d-flex">
                <div class="ps-3 pe-2 d-flex align-items-end">
                    <img style="width:30px;height:30px;border-radius:50%;object-fit:cover;" src="/img/${message.sender.profilePictureUrl}">
                </div>
                <div>
                    <div class="px-3 pt-2 pb-2 text-center" style="background:#f0f0f0;border-radius:20px;border:1px solid #dedede;min-width:50px">
                        <span>${message.content}</span>
                    </div>
                </div>
            </div>`;
        $(".mess-conversion").prepend(content);
    });

    window.addEventListener('beforeunload', function () {
        connection.stop().then(() => console.log("SignalR disconnected")).catch(console.error);
    });
}
function loadMessage() {
    if (isLoading) return; // Nếu đang tải thì không gọi tiếp
    isLoading = true; // Đánh dấu là đang tải

    $.ajax({
        url: `/Account/GetPosts?userName=${username}&pageIndex=${pageIndex}&pageSize=${pageSize}`,
        method: "GET",
        success: function (data) {
            let content = "";
            if (data.length === 0) {
                $(window).off("scroll"); // Không còn bài viết, tắt sự kiện cuộn

            } else {

            }
        }
        })
}
function addMessager() {
    const inputElement = document.querySelector('.mess-input');
    const inputValue = inputElement.value.trim();

    if (inputValue === "") {
        alert("Tin nhắn không được để trống!");
        return;
    }

    $.ajax({
        url: '/Messenger/AddMessage',
        method: 'POST',
        contentType: 'application/json', // Đảm bảo rằng dữ liệu được gửi dưới dạng JSON
        data: JSON.stringify({
            conversionId: id,
            content: inputValue
        }),
        success: function (response) {
            const content = `
                <div>
                    <div class="pb-3">
                        <div class="d-flex flex-row-reverse">
                            <div class="pe-3">
                                <div class="px-3 pt-2 pb-2 text-center" style="background:#f0f0f0;border-radius:20px;border:1px solid #dedede;min-width:50px">
                                    <span>${response.content}</span>
                                </div>
                            </div>
                            <div class="flex-grow-1"></div>
                        </div>
                    </div>
                </div>`;
            $(".mess-conversion").prepend(content);
            inputElement.value = ""; // Reset trạng thái ô input
            sendNotification(id)
        },
        error: function (error) {
            console.error("Error adding message:", error.responseText);
            alert("Có lỗi xảy ra khi gửi tin nhắn. Vui lòng thử lại.");
        }
    });
}

function sendNotification(conversationId) {
    console.log(conversationId)
    $.ajax({
        url: '/Notification/AddNotification',
        method: 'POST',
        contentType: 'application/json',
        data: conversationId,
        success: function () {
            console.log("Notification sent successfully for conversationId:", conversationId);
        },
        error: function (error) {
            console.error("Error sending notification for conversationId:", conversationId, error.responseText);
        }
    });
}

function loadMoreMessages() {

    $.ajax({
        url: `/messenger/GetMessageInConversation?Id=${id}&pageSize=${pageSizeMess}&pageNumber=${pageIndexMess}`,
        method: "GET",
        dataType: "json",
        success: function (messages) {
            if (messages.length > 0) {

                messages.forEach(mess => {
                    let messageHtml = '';

                    if (mess.isMine) {
                        messageHtml = `
                            <div class="pb-3">
                                <div class="d-flex flex-row-reverse">
                                    <div class="pe-3">
                                        <div class="px-3 pt-2 pb-2 text-center"
                                             style="background:#f0f0f0;border-radius:20px;
                                                    border:1px solid #dedede;min-width:50px">
                                            <span>${mess.content}</span>
                                        </div>
                                    </div>
                                    <div class="flex-grow-1"></div>
                                </div>
                            </div>`;
                    } else {
                        messageHtml = `
                            <div class="pb-3">
                                <div class="d-flex">
                                    <div class="ps-3 pe-2 d-flex align-items-end">
                                        <img id="avatarImage"
                                             style="width:30px;height:30px;border-radius:50%;
                                                    object-fit:cover;"
                                             src="/img/${mess.sender?.profilePictureUrl}">
                                    </div>
                                    <div>
                                        <div class="px-3 pt-2 pb-2 text-center"
                                             style="background:#f0f0f0;border-radius:20px;
                                                    border:1px solid #dedede;min-width:50px">
                                            <span>${mess.content}</span>
                                        </div>
                                    </div>
                                    <div class="flex-grow-1"></div>
                                </div>
                            </div>`;
                    }
                    // Thêm tin nhắn vào đầu danh sách
                    $(".mess-conversion").append(messageHtml);
                });
               
                console.log()

                // Tăng số trang
                pageIndexMess++;
                isLoading = false;
              
            }
        },
        error: function (error) {
            console.error("Lỗi tải tin nhắn:", error);
        },
    });
}


document.addEventListener('DOMContentLoaded', function () {
    initializeSignalR();
  
    $('#videoCallButton').on('click', function () {

        window.open(`https://1c76-1-54-8-29.ngrok-free.app/messenger/VideoCall?conversionId=${id}&isSend=true&sender=${currentUserName}&received=${users[0].userName}`, '_blank', "noopener,noreferrer,width=800,height=600");

        connection.invoke("SendInvitation", parseInt(id, 10), currentUserName).catch(console.error)
    });
    $("#send-message").on('click', addMessager);
    $(document).on("click", ".checkconversation", function () {
        var friendName = $(this).data("conversation"); // Lấy tên của bạn bè từ data attribute
        console.log(friendName); // Kiểm tra dữ liệu

        $.ajax({
            url: `/Messenger/CheckConversation?friendName=${friendName}`, // Đường dẫn API của bạn
            type: "POST",
            data: { friendName: friendName },
            success: function (response) {
                if (response.exists) {
                    window.location.href = "/Messenger/" + response;
                } else {
                    window.location.href = "/Messenger/" + response;
                }
            },
            error: function (xhr, status, error) {
                console.error("Lỗi khi kiểm tra cuộc trò chuyện:", error);
                alert("Đã xảy ra lỗi, vui lòng thử lại!");
            }
        });
    });
    $(".mess-conversion").on("scroll", function () {

        
        if ($(this).innerHeight() - $(this).scrollTop() >= $(this)[0].scrollHeight-1) {
            if (isLoading == true) {
                console.log(1)
                return;
            }
            isLoading = true;
            loadMoreMessages();
        }
    });

    $("#search-input1").on("input", function () {
        
        clearTimeout(timeout);
        let query = $(this).val().trim();

        if (query.length === 0) {

            isLoading = false;
            $(".row-custom").html("");
            pageIndex = 1;
 
            $(window).on("scroll");
            return;
        }

        $(window).off("scroll.loadFriendEvent");
        timeout = setTimeout(() => {
            $.ajax({
                url: `/Account/SearchFriends?query=${query}&userName=${currentUserName}`,
                type: "GET",
                success: function (data) {
                    let content = ""
                    console.log(data)
                    if (data.length === 0) {


                        return;
                    }
                    
                    data.forEach(user => {
                        content += `
                                     <div class="d-flex w-100 pt-2 pb-2 px-2 checkconversation" data-conversation="${user.userName}">
                                            <a class="w-100" asp-controller="Messenger" asp-action="Index" asp-route-Id="${user.id}" style="color:#333">
                                                <div class="d-flex px-2 bravt"><div class="mb-1 mt-1 me-3"><img style="width:45px;height:45px;border-radius:50%;object-fit:cover;" src="/img/${user.profilePictureUrl}"></div><div class="d-flex align-items-center"><span>${user.fullName}</span></div></div>
                                            </a>

                                     </div>`;
                    });

                    $(".conversion-container").html(content);
                },
                error: function (xhr, status, error) {
                    console.error("Lỗi khi tìm kiếm:", error);
                }
            });
        }, 300); // Debounce 300ms
    });

});