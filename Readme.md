# Dependencies
* Dotnet core
* Yarn
* Node.js

# Build
To build first build the client. It will output a single js file (`bundle.js`) in the `wwwroot` directory.

# Client Build
The first time you need to install dependencies

```
yarn install
```

To compile once you can use `webpack-cli`. To compile on change use `webpack --watch`.
To install both tools you can do the following:

```
yarn global add webpack-cli
yarn global webpack --watch
```

Don't forget to add yarn global directory to path so these executable can be found.

#Server Build

Just type 
```
dotnet run
```
To run the server or
```
dotnet watch run
```
to compile on change
TODO : How to publish on deploy on Google App Engine.