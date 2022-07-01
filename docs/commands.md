# Описсание комманд

## SignIn +
- Request: в `Data` хранится имя и пароль
- Response: в `Data` хранятся пользователь, контакти, группи, инвайти


## SingUp +
- Request: в `Data` хранится имя и хеширований пароль
- Response: в `Data` хранятся пользователь


## SendMessageToContact
- Request: в `Data` хранится id контакта, сообщение

Должно отправлятся 2-а response:
- Response тому кто отправил: в `Data` ничего, если успешно. Сообщение ошибки если ошибка.
- Response тому кому бил отправлен: в `Data` новое сообщение, смотреть `GetMessageFromContact`


## SendMessageToGroup
- Request: в `Data` хранится id групи, сообщение

Должно отправлятся 2-а response:
- Response тому кто отправил: в `Data` ничего, если успешно. Сообщение ошибки если ошибка.
- Response тому кому бил отправлен: в `Data` новое сообщение, смотреть `GetMessageFromGroup`


## GetMessageFromContact
Только:
- Response: в `Data` id контакта, само сообщение


## GetMessageFromGroup
Только:
- Response: в `Data` id групи, имя пользователя (которий отправил), само сообщение


## SendInvite +
- Request: в `Data` хранится id пользователя (которий сейчас залогинился), имя пользователя (которому надо отправить инвайт).

Должно отправлятся 2-а response:
- Response тому кто отправил инвайт: в `Data` ничего, если успешно. Сообщение ошибки если ошибка.
- Response тому кому бил отправлен инвайт: в `Data` новий инвайт


## GetInvite
Только:
- Response: в `Data` id инвайта (неподтвержденний контакт), имя пользователя, которий отправил


## AcceptInvite
- Request: в `Data` id контакта


## GetContact
Только:
- Response: в `Data` id контакта, имя контакта


## RenameContact
- Request: в `Data` хранится имя и хеширований пароль
- Response: в `Data` хранятся пользователь


## RemoveContact
- Request: в `Data` хранится имя и хеширований пароль
- Response: в `Data` хранятся пользователь


## AddGroup
- Request: в `Data` хранится имя и хеширований пароль
- Response: в `Data` хранятся пользователь


## LeaveGroup
- Request: в `Data` хранится имя и хеширований пароль
- Response: в `Data` хранятся пользователь


## RenameGroup
- Request: в `Data` хранится имя и хеширований пароль
- Response: в `Data` хранятся пользователь

## Disconnect
- Request: в `Data` хранится id
```Data = 1```