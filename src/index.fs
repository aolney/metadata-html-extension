module App

// open FSharp.Core
open Fable.Core
// open Fable.Core.JS
open Fable.Core.JsInterop
// open Fable.Promise
open Browser.Types
open Browser.Dom
open JupyterlabNotebook.Tokens

//tricky here: if we try to make collection of requires, F# complains they are different types unless we specify obj type
let mutable requires: obj array =
    [|  JupyterlabNotebook.Tokens.Types.INotebookTracker |]


/// Catch the notebook revealed event. Only then are the cells "ready"
let injectHtml( notebook : NotebookPanel ) = 
  console.log("html injection")
  let cells = notebook.content.widgets
  for i = 0 to cells.length - 1 do
  let cell = cells.[i]
  if cell.model.``type`` = JupyterlabCoreutils.Nbformat.Nbformat.CellType.Markdown then
    let markdownCell = cell :?> JupyterlabCells.Widget.MarkdownCell
    // markdownCell.model.trusted <- true
    let jsonOption = markdownCell.model.metadata.get("html")
    if jsonOption.IsSome then
      //some navigation in the DOM to align our HTML with the rendered markdown; likely fragile to future changes
      let inputWrapper = markdownCell.node.children.[1] :?> HTMLElement
      let inputArea = inputWrapper.children.[1] :?> HTMLElement
      //check for previous injection to avoid duplication
      let lastChildElement = inputArea.lastChild :?> HTMLElement
      if not <| lastChildElement.classList.contains("metadata-html") then
        let target = inputArea.children.[2] :?> HTMLElement
        target.insertAdjacentHTML("beforeend",jsonOption.Value.ToString())


/// On every notebook change, register a promise to display when the notebook is revealed.
/// Cells are not accessible at this time point, so we have to catch this promise
let onNotebookChanged =
          PhosphorSignaling.Slot<JupyterlabApputils.IWidgetTracker<NotebookPanel>, NotebookPanel option >(fun sender args -> 
            console.log("notebook changed")
            match sender.currentWidget with
            | Some( notebook ) -> 
              notebook.revealed.``then``( fun _ -> injectHtml(notebook) ) |> ignore
            | None -> ()
            //
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
              
              //catch when notebook changes
              notebooks.currentChanged.connect( onNotebookChanged, null ) |> ignore

            ) //System.Func
        ]

exportDefault extension
