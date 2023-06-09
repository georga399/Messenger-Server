# Messenger-Server

Functionality:
-------
There are 2 types of chat:
- Private (Only Admin can add user to chat, only admin can delete someone else's message)
- Group (All users can join to this chat, all users can delete someone else's message)

Server REST API:
-------
|API                    |Description            |Request body   |Response body       |
|:----------------------|:----------------------|:--------------|:-------------------|
|POST /api/auth/register |Registration new users |  JSON: {"password": "string","userName": "string","email": "user@example.com"}| Code: 200 - success|
|POST /api/auth/login | User's authorization | JSON: {"password": "string","userName": "string"} | Code: 200 - success |
|GET /api/auth/logout | Logout user | None | Code: 200 - success |
|GET /api/user/getuserinfo/{id} |Info about this user| None|Code: 200 JSON: UserViewModel| 
|POST /api/user/ava |Set avatar for user |Stream of bytes| Code: 200 string - URI for downloading file|
|GET /api/user/ava/{id}| Get file avatar of user |None | Code: 200 Stream of bytes|
|DELETE /api/user/deleteuser |Deleting  current user| None | Code: 200|
|GET /api/chat/getchatinfo/chatid={id:int} |Get info about this chat | None | Code: 200 JSON: ChatViewModel |
|GET /api/chat/getuserschats | Get all chats with current user| None |Code: 200 JSON: Array of ChatViewModels |
|POST /api/chat/chatava/{chatId} | Set avatar of chat | Stream of bytes | Code: 200 String: URI for downloading file |
|GET /api/chat/chatava/{chatId:int} | Get avatar of chat | None| Code: 200 Stream of bytes |
|GET /api/chat/getallmessages/chatid={id:int} | Get all messages of chat | None| Code: 200 JSON: Array of MessageViewModels|
|GET api/messages/getmessagesrange/chatid={chatid:int}/frommsgid={messageid:int}-range={range:int} | Get range of messages. Beginning from messageid and with count range. If range<0 then messages download in reverse | None | Code: 200 JSON: Array of MessageViewModels|
|GET /api/messages/getlastreadmessageid/chatid={chatid} | Get id of the last read  message| None | Code: 200. Integer value|
|GET /api/messages/getnewestmessageid/chatid={chatid} | Get id of newest message in chat | None| Code: 200. Integer value|
|POST /api/upload/atach | Upload file to the server| Stream of bytes | Code: 200. String value: URI for downloading file |
|GET /api/upload/attach/{fileName} |Download file from the server| None |Code: 200. Stream of bytes |
|DELETE /api/upload/attach/{filename}| Delete file on the server| None | Code: 200.|

MessageHub's API (/chat)
------
|Method signature (Server) |Description      |Callback (Client) |Callback invocation |
|:-------------------------|:----------------|:-----------------|:-------------------|
| SendMessage(int chatId, MessageViewModel messageViewModel) |Send message to the chat | OnSendMessage(messageViewModel) |All users in this chat|
|DeleteMessage(int chatId, int messageId) | Delete message in chat| OnDeleteMessage(int chatId, int messageId)| All users in this chat |  
|CreateChat(ChatViewModel chat) |Create chat. Added members to the chat. Set current user as admin. | OnJoinChat(ChatViewModel chat)| All users in this chat|
|JoinChat(int chatId, int userId) | Add user to the chat | OnJoinChat(ChatViewModel chat) | user that was added to the chat
| | | OnAddedUserToChat(userId) - all users in the chat| all users of the chat|
|LeaveChat(int chatid) | Delete user from the chat| OnLeaveChat(string $"UserId={userId} left ChatId={chatid}) | Caller |
|SetLastReadMessage(int chatId, int messageId) | Set last read message for current user | OnSetLastReadMessage(chatId, messageId) | Caller|

With some incorrect request on client will invoke OnError(string errorMsg)# Example
