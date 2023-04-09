# Messenger-Server

Server API:
-------
|API                    |Description            |Request body   |Response body       |
|:----------------------|:----------------------|:--------------|:-------------------|
|Post /api/auth/register |registration new users |  JSON: {"password": "string","userName": "string","email": "user@example.com"}| Code: 200 - success|
|Post /api/auth/login | user's authorization | JSON: {"password": "string","userName": "string"} | Code: 200 - success |
|Get /api/auth/logout | logout user | None | Code: 200 - success |


MessageHub's functionality (/chat)
------
|Method signature (Server) |Description      |Callback (Client) |Callback invocation |
|:-------------------------|:----------------|:-----------------|:-------------------|
|? | ? | ? | ? |
