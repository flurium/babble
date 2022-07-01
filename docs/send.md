# Формат отправки данних

## Структури
Все структури находятся в CrossLibrary. Для работи с ними надо вставить `using CrossLibrary;` в начало файла.

Есть enum Command, которий отображает отправляемие команди. 
```cs
enum Command {
  SingIn,
  SingUp,
  SendMessageToContact,
  SendMessageToGroup,
  Invite,
  AcceptInvite,
  RenameContact,
  RemoveContact,
  AddGroup,
  LeaveGroup,
  RenameGroup
  // RemoveGroup не нужен, так как група автоматически удаляется если ее покнут все учасники
}
```

Есть enum Status, которий отоюражает статус в response.
```cs
enum Status {
  OK,
  Bad
}
```

Структура запроса:
```cs
struct Request
{
  Command Command { get; set; }
  dynamic Data { get; set; }
}
```

Структура ответа:
```cs
struct Response
{
  Status Status { get; set; }
  Command Command { get; set; }
  dynamic Data { get; set; }
}
```

Структура которая описивает большинство сущностей, которие отправляются в поле `Data` от сервера:
```cs
struct Prop {
  int Id { get; set; }
  string Name { get; set; }
}
``` 


## Пример отправки запроса на регистрацию:

```cs
Request req = new Request {
  Command = Command.SignUp,
  Data = new {
    Name = "username",
    Password = "password"
  } 
};
```

То что получится после JsonConvert.Serialyze(req)
```json
{
 "Command": 1,
 "Data": {
  "Name": "username",
  "Password": "password"
 }
}
```
Ви видете что команда по факту ето число, но когда ви будете десериализовать ето в обьект типа `Request` комманда снова станет типа `enum Command`. Сравнение производите так:
```cs
if(req.Command == Command.SingIn) {
  ...
}
```

## Пример отправки результата на запрос регистрации 
То есть на сервер пришел запрос на регистрацию, он виполнил операцию и ето то что он вернет на клиент:
  
```cs
var user = db.AddUser(req.Data.Name, req.Data.Password)

Response res = new Response {
  Status = Status.OK,
  Command = req.Command,
  Data = new Prop {
    Id = user.Id,
    Name = user.Name
  }  
}
```

## Старая инфа

### Пример отправки запроса на логин: 
```json
{
  "command":"signin",
  "data": {
    "name": "имя пользователя",
    "password": "не хеширований пароль"
  }
}
```

### Пример отправки запроса на отправку сообщения:

- `from` = id пользователя которий отправляет
- `to` = id пользователя которий получит сообщение

```json
{
  "command":"send",
  "data": {
    "from": 5, 
    "to": 6,
    "message": "какое-то сообщение"
  }
}
```


## От сервера на клиент
```json
{
  "status": "ok",
  "data": {
    "user" : {},
    "contacts": [
      {
        "id": 6,
        "name": "имя контакта"
      },
    ],
    "groups": [
      {
        "id": 2,
        "name": "название группи"
      },
    ],
    "invitations": [
      {
        "id": 1,
        "name": "имя пользователя, которий прислал приглашение"
      },
    ]
  }
}
```
