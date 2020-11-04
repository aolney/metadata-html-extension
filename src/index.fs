module App

// open FSharp.Core
open Fable.Core
// open Fable.Core.JS
open Fable.Core.JsInterop
// open Fable.Promise
open Browser.Types
open Browser.Dom
// open JupyterlabServices.__kernel_messages.KernelMessage
// open JupyterlabServices.__kernel_kernel.Kernel
open JupyterlabNotebook.Tokens

//tricky here: if we try to make collection of requires, F# complains they are different types unless we specify obj type
let mutable requires: obj array =
    [|  JupyterlabNotebook.Tokens.Types.INotebookTracker |]

// [<Emit("typeof $0")>]
[<Emit("$0.constructor.name")>]
let jsTypeof (_ : obj) : string = jsNative

let mutable currentNotebook  : NotebookPanel option = None

/// Catch the notebook revealed event. Only then are the cells "ready"
let injectHtml( ) = //notebooks: JupyterlabNotebook.Tokens.INotebookTracker) =
  console.log("html injection")
  // currentNotebook <- notebooks.currentWidget
  if currentNotebook.IsSome then
    let cells = currentNotebook.Value.content.widgets
    for i = 0 to cells.length - 1 do
    let cell = cells.[i]
    if cell.model.``type`` = JupyterlabCoreutils.Nbformat.Nbformat.CellType.Markdown then
      let markdownCell = cell :?> JupyterlabCells.Widget.MarkdownCell
      markdownCell.model.trusted <- true



/// On every notebook change, register a promise to display when the notebook is revealed.
/// We have to catch this promise because the cells are not accessible at this time point
let onNotebookChanged =
          PhosphorSignaling.Slot<JupyterlabApputils.IWidgetTracker<NotebookPanel>, NotebookPanel option >(fun sender args -> 
            console.log("notebook changed")
            match sender.currentWidget with
            | Some( notebook ) -> 
              currentNotebook <- Some(notebook)
              notebook.revealed.``then``( fun _ -> injectHtml() ) |> ignore
            | None -> currentNotebook <- None
          
          // PhosphorSignaling.Slot<JupyterlabApputils.IWidgetTracker<NotebookPanel>, NotebookPanel >(fun sender args -> 
          // PhosphorSignaling.Slot<JupyterlabApputils.IWidgetTracker<NotebookPanel>, NotebookPanel option >(fun sender args -> 
              // if sender.currentWidget.IsSome then
              //   let cells = sender.currentWidget.Value.content.widgets
              //   for i = 0 to cells.length - 1 do
              //   let cell = cells.[i]
              //   if cell.model.``type`` = JupyterlabCoreutils.Nbformat.Nbformat.CellType.Markdown then
              //     let markdownCell = cell :?> JupyterlabCells.Widget.MarkdownCell
              //     markdownCell.model.trusted <- true
          // PhosphorSignaling.Slot<JupyterlabApplication.LabShell,JupyterlabApplication.ILabShell.IChangedArgs >(fun sender args -> 
          //   console.log( "sender is " + (jsTypeof sender) )
          //   console.log( "new arg is " + ( jsTypeof args.newValue ) )
            
          //   if args.newValue.IsSome && jsTypeof args.newValue.Value = "NotebookPanel" then
          //     let notebookPanel =  sender.activeWidget.Value :?> NotebookPanel
          //     let cells = notebookPanel.content.widgets
          //     for i = 0 to cells.length - 1 do
          //     let cell = cells.[i]
          //     if cell.model.``type`` = JupyterlabCoreutils.Nbformat.Nbformat.CellType.Markdown then
          //       let markdownCell = cell :?> JupyterlabCells.Widget.MarkdownCell
          //       markdownCell.model.trusted <- true
          //     ()

            true
          )



let extension =
    createObj
        [ "id" ==> "metadata_html_extension"
          "autoStart" ==> true
          "requires" ==> requires //
          //------------------------------------------------------------------------------------------------------------
          //NOTE: this **must** be wrapped in a Func, otherwise the arguments are tupled and Jupyter doesn't expect that
          //------------------------------------------------------------------------------------------------------------
          "activate" ==> System.Func<JupyterlabApplication.JupyterFrontEnd<JupyterlabApplication.LabShell>, JupyterlabNotebook.Tokens.INotebookTracker, unit>(fun app notebooks ->
              console.log ("JupyterLab extension metadata_html_extension is activated!")
              
              // app.restored.``then``(fun _ -> injectHtml(notebooks)  ) |> ignore

              // )
              //does not fire
              // notebooks.widgetUpdated.connect( onNotebookChanged, null ) |> ignore
              // notebooks.widgetAdded.connect( onNotebookChanged, null ) |> ignore

              //fires but does not have all cells listed on page reload
              notebooks.currentChanged.connect( onNotebookChanged, null ) |> ignore

              //fires but does not have all cells listed on page reload
              // app.shell.currentChanged.connect( onNotebookChanged, null) |> ignore
            ) //System.Func
        ]

exportDefault extension
