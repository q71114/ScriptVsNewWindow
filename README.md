# ScriptVsNewWindow

Reproduction of timing issue when setting `CoreWebView2NewWindowRequestedEventArgs.NewWindow` vs ensuring scripts are loading prior to navigation (i.e. `CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync`).

# Expected Behavior
The project loads an embedded web page with a button and a link. When either are clicked a new window should appear containing a WebView2 instance that:
  1. Has scripts added (via a call to `CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync`) that cause an alert to be shown on document load.
  2. Is assigned to the `NewWindow` property of `CoreWebView2NewWindowRequestedEventArgs`
  3. Loads to destination url (https://ian.bebbs.co.uk)
  4. Fires the JS alert twice (once for core document load, once for an embedded frame).

# Actual Behavior
Behavior is indeterminate as it seems there is a race condition. Either of the following are common outcomes:
  1. A blank web page is displayed with scripts added but no alerts as no document is loaded.
  2. The destination url is displayed but no alerts are displayed as the scripts have not been added.

# Requirements
This project requires:
  1. .NET6 SDK
  2. An evergreen deployment of WebView2 - at time of writing this is runtime version '107.0.1418.42'.

# Setup
The project contains numerous modifiers at the top of the main window. These include:
  1. Set New Window - Determines whether the new WebView2 instance should be assigned to the `NewWindow` property of `CoreWebView2NewWindowRequestedEventArgs`
  2. Add Scripts - Determins if and when to call `CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync`:
      * After - will be called after assigning `CoreWebView2NewWindowRequestedEventArgs.NewWindow` (per documentation [here](https://learn.microsoft.com/en-us/microsoft-edge/webview2/reference/win32/icorewebview2newwindowrequestedeventargs?view=webview2-1.0.1418.22#put_newwindow))
      * Before - will be called before assigning `CoreWebView2NewWindowRequestedEventArgs.NewWindow` (per documentation [here](https://learn.microsoft.com/en-us/dotnet/api/microsoft.web.webview2.core.corewebview2newwindowrequestedeventargs.newwindow?view=webview2-dotnet-1.0.1418.22#microsoft-web-webview2-core-corewebview2newwindowrequestedeventargs-newwindow))
      * None - `CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync` will not be called.
  3. Delay - Adds an optional `await Task.Delay(<milliseconds>))` after assigning `CoreWebView2NewWindowRequestedEventArgs.NewWindow` as this has been found to change behavior
  4. Set Source - Specifically assigns the `CoreWebView2NewWindowRequestedEventArgs.Uri` to the new WebView's `Source` property.
  5. Schedule New Window - Allows the `CoreWebView2NewWindowRequested` event to complete by scheduling a new action onto the dispatcher (aka UI) thread which performs the initialization of the new Window/WebView

# Reproduction Steps
## Expected behavior by not assigning `CoreWebView2NewWindowRequestedEventArgs.NewWindow`
(See ![Expected Behavior](Recordings/Expected Behavior.gif | width=100))

1. Start the app.
2. Modify controls to the following:
    * "Set New Window" __unchecked__
    * "Add Scipts" set to "After"
    * "Delay" set to "None"
    * "Set Source" __checked__
    * "Schedule New Window" unchecked
3. Click the 'Open Window From Target="_blank"' link.
4. A new window will appear and two JS alerts shown as the content loads.

## Actual behavior by assigning `CoreWebView2NewWindowRequestedEventArgs.NewWindow`
(See ![Actual Behavior](Recordings/Actual Behavior.gif | width=100) & ![Actual Behavior with Delay](Recordings/Actual Behavior with Delay.gif | width=100))
1. Start the app.
2. Modify controls to the following:
    * "Set New Window" __checked__
    * "Add Scipts" set to "After"
    * "Delay" set to "None"
    * "Set Source" __unchecked__
    * "Schedule New Window" unchecked
2. Click the 'Open Window From Target="_blank"' link.
3. A new window will appear which either:
    1. Is blank
    2. Navigates to the destination URL but no JS alerts are shown; i.e. scripts have not been added prior to first document load.
    3. Navigates to the destination URL but only a single JS alerts is shown; i.e. scripts were not added prior to first document load.
4. Try various settings for "Delay" and observe different outcomes in the previous step