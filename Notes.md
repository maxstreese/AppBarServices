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

### When to hide and unhide
* Use the MouseEnter and MouseLeave events of the handled window. No WinApi functions required.

## Questions come and go
* One question is how I have to implement all of the steps with regards to the AutoHide functionality, the other is how I can test whether it worked. "-->" There is two sides to the implementation: One, building all the functionality to talk to the operating system and two, communicating with the handled window (e.g. getting notified when the mouse is over the hidden AppBar)
* What kind of options/behavoir should there be regarding the AutoHide-toggle? Options:
  * When the AppBar is hidden, it will show itself again when the mouse hovers above it. It will hide itself, once the mouse leaves the window.
  * The AppBar will only show itself when it is clicked.

## Stuff to do
* All _currentAppBarAttributes should be reset when they are no longer correct. This includes setting the margins to 0 and adding a "None" to the ScreenEdge enum. I should also ask Ralf about this.
