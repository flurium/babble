```mermaid
graph TD;
    from[ClientFrom]
    to[ClientTo]
    server[Server]
    
    from -. ask for file send<br/>buffer size .-> server
    server == buffer size ==> to
    server -- ClientTo address ---> from
    
```


```mermaid
sequenceDiagram
    participant ClientFrom
    participant Server
    participant ClientTo

    Note over ClientTo: TCP listener already started
    
    ClientFrom ->> Server: Ask for file send, buffer size
    alt ClientTo is online 
        Server ->> ClientTo: buffer size
        Note left of ClientTo: Set TCP listener buffer
        Server ->> ClientFrom: ClientTo adress 
        ClientFrom ->> ClientTo: file message
    else ClientTo is offline
        Server ->> ClientFrom: ClientTo is not online
    end
    

```