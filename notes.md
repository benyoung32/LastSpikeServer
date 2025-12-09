connection protocol:
    landing page with name input
    create session || join session
    
    create session:
        creates new session, then joins it
    join session:

    

when gamestate changes, send update to all clients in the corresponding session

    client posts change to gameboard
    server validates change
    server updates gameboard
    server notifies clients that the gameboard has changed
    clients fetch new gameboard state


create a new session:
    client posts new session
    client gets session id
    client joins session
    client gets url with session id to share
    server posts gameboard
    

turn phases:
    start - move, trade
    confirm space landed on - accept, pass, trade 
        if track or rebellion, addtrack, rebellion
    end