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


## AcceptInvite +
- Request: в `Data` id контакта


## GetContact
Только:
- Response: в `Data` id контакта, имя контакта


## RenameContact
- Request: в `Data` id from, id to, new name
- Response: в `Data` 


## RemoveContact
- Request: в `Data` id from, id to
- Response: в `Data`

## AddGroup
- Request: в `Data` name, user id
- Response: в `Data` 


## LeaveGroup
- Request: в `Data` uid, group id
- Response: в `Data`


## RenameGroup
- Request: в `Data` group id, name
- Response: в `Data` 

## Disconnect
- Request: в `Data` хранится user id
```Data = 1```