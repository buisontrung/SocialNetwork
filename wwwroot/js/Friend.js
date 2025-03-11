let pageIndex = 1;
const pageSize = 7;
let isLoading = false;

function loadFriendsPending() {
    if (isLoading) return;
    console.log(1)
    isLoading = true;
    $.ajax({
        url: `/Friend/GetFriendsPending?pageIndex=${pageIndex}&pageSize=${pageSize}`, // Đường dẫn API của bạn
        method: 'GET',
        success: function (data) {
            if (data.length < pageSize) {
                $('#loadFriendsButton').remove();
            }
            var friendsList = $('.row-custom-friend');
            let content = '';
            data.forEach(function (friend) {
                content +=`
                    <div class="friend-div box">
                        <div class="">
                            <img class="w-100" src="/img/${friend.profilePictureUrl}" alt="Alternate Text" />
                        </div>
                        <div class="p-2 bdf">
                            <div class="text-start mt-1 mb-1">
                                <span style="font-size:19px">${friend.fullName}</span>
                            </div>
                            <div class="d-flex mb-2">
                                <div class="d-flex me-1" style="min-height:32px">
                                ${friend.commonFriends.map(friend =>
                                    `<div class="friend-avatar"><a class="dropdown-item" href="/Profile/${friend.userName}"><img src="/img/${friend.profilePictureUrl}" class="friend-img"></a></div>`
                                    ).join('')

                                    
                                }
                                   
            

                                </div>
                                <div class="d-flex align-items-center">
                                    <span style="font-size:14px">${friend.commonFriendsCount} bạn chung</span>
                                </div>
                            </div>
                            <div class="btnfrad">
                                <button class="w-100 p-1 mb-2 confirm-button" style="border-radius:5px;background:#0866ff;color:#fff;font-size:16px" data-friendrequest="${friend.friendRequestId}">Xác nhận</button>
                                <button class="w-100 p-1 delete-friend-button" style="border-radius:5px;background:#dedede;color:#333;font-size:16px" data-friendrequest="${friend.friendRequestId}">Xóa</button>
                            </div>
                        </div>
                    </div>`
                
            });
            friendsList.append(content)
            isLoading = false;
            pageIndex++; // Tăng pageIndex để tải trang tiếp theo
        },
        error: function () {
            alert('Failed to load pending friends.');
            isLoading = false;
        }
    });
}
function loadRecommend() {

    $.ajax({
        url: `/Friend/GetRecommendFriends`, // Đường dẫn API của bạn
        method: 'GET',
        success: function (data) {
 
            var friendsList = $('.recommend-friend');
            let content = '';
            data.forEach(function (friend) {
                content += `
                    <div class="friend-div box">
                        <div class="">
                            <img class="w-100" src="/img/${friend.profilePictureUrl}" alt="Alternate Text" />
                        </div>
                        <div class="p-2 bdf">
                            <div class="text-start mt-1 mb-1">
                                <span style="font-size:19px"><a href="/Profile/${friend.userName}">${friend.fullName}</a></span>
                            </div>
                            <div class="d-flex mb-2">
                                <div class="d-flex me-1" style="min-height:32px">
                              
                                   
            

                                </div>
                                <div class="d-flex align-items-center">
                                    <span style="font-size:14px"> bạn chung</span>
                                </div>
                            </div>
                            <div class="btnfrad">
                                <button class="w-100 p-1 mb-2 confirm-button" style="border-radius:5px;background:#0866ff;color:#fff;font-size:16px" data-friendrequest="${friend.id}">Thêm bạn</button>

                            </div>
                        </div>
                    </div>`

            });
            friendsList.append(content)
            isLoading = false;
            pageIndex++; // Tăng pageIndex để tải trang tiếp theo
        },
        error: function () {
            alert('Failed to load pending friends.');
            isLoading = false;
        }
    });
}
document.addEventListener('DOMContentLoaded', function () {
    // Load trước 7 người bạn
    loadFriendsPending();
    // Bắt sự kiện click cho nút "Tải thêm bạn bè"
    $('#loadFriendsButton').on('click', loadFriendsPending);
    $(document).on('click', '.confirm-button', function () {
        const friendRequestId = $(this).data('friendrequest')
        const $button = $(this);
        $.ajax({
            url: '/Friend/AcceptFriend',
            method: 'POST',
            data: { Id: friendRequestId },
            success: function (response) {
                $button.closest('.friend-div').find('.btnfrad').html(`
                <button class="w-100 p-1" style="border-radius:5px;background:#dedede;color:#333;font-size:16px">Đã chấp nhận</button>
            `);
            },
            error: function () {
                alert('Failed to accept friend request.');
            }
        });
    });
    $(document).on('click', '.delete-friend-button', function () {
        const friendRequestId = $(this).data('friendrequest')
        const $button = $(this);
        $.ajax({
            url: '/Friend/RemoveFriend',
            method: 'DELETE',
            data: { Id: friendRequestId },
            success: function (response) {
                $button.closest('.friend-div').remove();
            },
            error: function () {
                alert('Failed to accept friend request.');
            }
        });
    });
    loadRecommend();
});