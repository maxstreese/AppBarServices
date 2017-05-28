## How to implement an AutoHide AppBar

### Overview
1. Register a window as a "normal" AppBar using the ABM_NEW message.
2. Register the same window as an AutoHide AppBar using ABM_SETAUTOHIDEBAREX.
3. Now do all the positioning associated with the AutoHide AppBar manually using the standard ABM_QUERYPOS and ABM_SETPOS messages.

* Public methods and set-properties
  * PlaceAppBar
  * MoveAppBar
  * RemoveAppBar
  * AutoHide
  * VisibleMargin
  * HiddenMargin

## Questions come and go
* One question is how I have to implement all of the steps with regards to the AutoHide functionality, the other is how I can test whether it worked. "-->" There is two sides to the implementation: One, building all the functionality to talk to the operating system and two, communicating with the handled window (e.g. getting notified when the mouse is over the hidden AppBar)
