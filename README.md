# metadata-html-extension

> [!WARNING]
> The repository is for JLab 1.2x and has been superceded by [another repo for JLab 4x](https://github.com/aolney/jupyterlab-metadata-html-extension)

A [JupyterLab](https://jupyterlab.readthedocs.io/en/stable/) extension with [Fable](https://fable.io/) tooling that uses cell metadata to define html that should be injected into markdown cells.

This approach overcomes the limitations of JupyterLab markdown cells for certain types of html, such as iframes, that appear to be stripped/sanitized based on the JupyterLab security model.

Using this extension therefore increases the likelihood that an attacker may use a notebook to execute arbitrary code on your computer.

This extension is meant for research purposes only and is not meant for general usage. 

Obviously, notebooks with html in the metadata will not render properly without this extension. 

Example metadata:

```javascript
{
    "html": "<iframe class='metadata-html' width='560' height='315' src='https://www.youtube.com/embed/nBrKsT1IvIM' frameborder='0' allow='accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture' allowfullscreen></iframe>"
}
```

`class='metadata-html'` will prevent duplicate html injection if switching between notebooks.

**NOTE: This plugin requires jupyterlab <= 1.2.6, so if you have a higher version (e.g. 2.X) you will need to execute `conda install jupyterlab=1.2.6` or similar for `pip`**


## Installation

```bash
jupyter labextension install @aolney/metadata-html-extension
```

## Updating to latest version

```bash
jupyter labextension update @aolney/metadata-html-extension
```

## Development

This is based on my personal preferences. For more options, [see the extension development guide](https://jupyterlab.readthedocs.io/en/stable/developer/extension_dev.html#developer-extensions).

### Prerequisites

* [JupyterLab](https://jupyterlab.readthedocs.io/en/stable/getting_started/installation.html)
* [Fable](https://fable.io/)
* An F# editor like Visual Studio Code with [Ionide](http://ionide.io/) 
* Chrome

### Initial install and after library adds

```bash
npm install
mono .paket/paket.exe install
npm run build
```

### Terminal A in VSCode

```bash
jupyter labextension install . --no-build
npm run watch
```

This will watch your F# code and trigger builds of `index.js`.

If you prefer not to trigger builds using a watch, you can `npm run build` every time you want a new build.

### Terminal B in VSCode

```bash
jupyter lab --watch
```

This will watch your extension and trigger builds of it.

Even with this watch, you still need to refresh your browser during development.

## Project structure

### npm/yarn

JS dependencies are declared in `package.json`, while `package-lock.json` is a lock file automatically generated.

### paket

[Paket](https://fsprojects.github.io/Paket/) 

> Paket is a dependency manager for .NET and mono projects, which is designed to work well with NuGet packages and also enables referencing files directly from Git repositories or any HTTP resource. It enables precise and predictable control over what packages the projects within your application reference.

.NET dependencies are declared in `paket.dependencies`. The `src/paket.references` lists the libraries actually used in the project. Since you can have several F# projects, we could have different custom `.paket` files for each project.

Last but not least, in the `.fsproj` file you can find a new node: `	<Import Project="..\.paket\Paket.Restore.targets" />` which just tells the compiler to look for the referenced libraries information from the `.paket/Paket.Restore.targets` file.

### Fable-splitter

[Fable-splitter]() is a standalone tool which outputs separated files instead of a single bundle. Here all the js files are put into the `lib`. And the main entry point is our `index.js` file.

### Imports

Because Jupyter uses Typescript, we can use [ts2fable](https://github.com/fable-compiler/ts2fable) to generate strongly typed imports of Jupyter's JS packages. Unfortunately these are a bit huge and the conversion is messy. 
I might release them as a nuget package once they are cleaned up.
`ts2fable-raw-output` has the initial conversion which is cleaned up enough to compile i nthe `src` directory. 



