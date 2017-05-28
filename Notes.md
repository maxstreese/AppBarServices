## How to implement an AutoHide AppBar
1. Register a window as a "normal" AppBar using the ABM_NEW message.
2. Register the same window as an AutoHide AppBar using ABM_SETAUTOHIDEBAREX.
3. Now do all the positioning associated with the AutoHide AppBar manually using the standard ABM_QUERYPOS and ABM_SETPOS messages.