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
* When the AutoHide AppBar unhides, it should not register a new AppBar position. Instead the unhidden AppBar should overlap other Applications. What does this entail?
  * Code changes
  * A new attribute (opacity)

## Questions come and go
* One question is how I have to implement all of the steps with regards to the AutoHide functionality, the other is how I can test whether it worked. "-->" There is two sides to the implementation: One, building all the functionality to talk to the operating system and two, communicating with the handled window (e.g. getting notified when the mouse is over the hidden AppBar)
* What kind of options/behavoir should there be regarding the AutoHide-toggle? Options:
  * When the AppBar is hidden, it will show itself again when the mouse hovers above it. It will hide itself, once the mouse leaves the window.
  * The AppBar will only show itself when it is clicked.

## Stuff to do
* All _currentAppBarAttributes should be reset when they are no longer correct. This includes setting the margins to 0 and adding a "None" to the ScreenEdge enum. I should also ask Ralf about this.
* Setting the AppBar to the bottom of the screen (i.e. where the taskbar is) leads to a short visual glitch, when the AppBar is set to AutoHide. I believe this comes from the GetPosSetPos handler method.

## Session - 2017-05-30
* Use the [MonitorFromWindow function](https://msdn.microsoft.com/en-us/library/dd145064(v=vs.85).aspx) rather than MonitorFromRect.
* Handle methods and what the should do
  * HandleAppBarNew
    * Registers the window as an AppBar
    * Sets isRegistered to true
  * HandleAppBarRemove
    * Unregisters the window as an AppBar
    * Sets isRegistered to false
  * HandleAppBarQueryPosSetPos
    * Queries and then reserves the position for the AppBar
    * Also positions the AppBar via HandleMoveWindow
    * Sets isHidden to true or false
    * Sets the currentMonitor attributes
    * Sets the currentPosition attributes
  * HandleGetMonitorInfoFromWindow
    * 
  * HandleMoveWindow
  * ProcessWinApiMessages

* PlaceAppBar(isAutoHide, screenEdge, visibleMargin, hiddenMargin)
  * if(!_isRegistered)
    * _windowSource = ...
    * screenEdge = ...
    * visibleMargin = ...

## Session 2017-06-03
* nothing to report here

## Session 2017-06-04
* -

## Session 2017-06-14
* 